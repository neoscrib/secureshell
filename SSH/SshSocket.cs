using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using SSH.Encryption;
using SSH.IO;
using SSH.Mac;
using SSH.Packets;
using SSH.Collections;

namespace SSH
{
    class SshSocket
    {
        TcpClient socket;
        NetworkStream st;
        PacketReader pr;

        Thread reader;
        Thread processor;
        Session session;
        BlockingQueue<IPacket> queue;

        Cipher decryptor;

        internal Mode SocketModeReceive { get; set; }
        internal Mode SocketModeSend { get; set; }
        internal Cipher Encryptor { get; set; }
        internal Cipher Decryptor { get { return decryptor; } set { decryptor = value; decryptorSetWaitHandle.Set(); } }
        internal MacActivator EncryptorMac { get; set; }
        internal MacActivator DecryptorMac { get; set; }
        internal uint ServerSequence { get; set; }
        internal uint ClientSequence { get; set; }

        private long bytesTx = 0;
        private long bytesRx = 0;

        private EventWaitHandle writerWaitHandle;
        private EventWaitHandle readerWaitHandle;
        private EventWaitHandle decryptorSetWaitHandle;

        internal SshSocket(Session session)
        {
            this.decryptorSetWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.writerWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
            this.readerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.session = session;
            socket = new TcpClient();
            SocketModeReceive = Mode.Ascii;
            SocketModeSend = Mode.Ascii;
        }

        internal StatusCode Connect(string hostname, int port)
        {
            var async = socket.BeginConnect(hostname, port, null, null);
            var success = async.AsyncWaitHandle.WaitOne(2000, true);
            if (!success) return SSH.StatusCode.ConnectionTimedOut;
            if (!socket.Connected) return SSH.StatusCode.ConnectionRefused;

            st = socket.GetStream();
            pr = new PacketReader(st);

            queue = new BlockingQueue<IPacket>();
            processor = new Thread(new ThreadStart(ProcessQueue));
            processor.Start();
            reader = new Thread(new ThreadStart(ReadPackets));
            reader.Start();

            return StatusCode.OK;
        }

        internal void Disconnect()
        {
            try
            {
                reader.Abort();
                reader.Join();
            }
            catch (NullReferenceException) { }

            try
            {
                processor.Abort();
                processor.Join();
            }
            catch (NullReferenceException) { }

            socket.Close();
            Debug.WriteLine("Transferred: Sent {0}; Received {1}", Util.ToHumanReadableSize(bytesTx), Util.ToHumanReadableSize(bytesRx));
        }

        private void ProcessQueue()
        {
            try
            {
                while (true)
                {
                    var p = queue.Dequeue();
                    if (p != null) session.Processors.Process(p);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("Queue processor thread aborting.");
                Thread.ResetAbort();
            }
        }

        private void ReadPackets()
        {
            try
            {
                while (socket.Connected)
                {
                    var p = PrivateReadPacket();
                    //if (p != null) session.Processors.Process(p);
                    queue.Enqueue(p);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("Reader thread aborting.");
                Thread.ResetAbort();
            }
        }

        private IPacket PrivateReadPacket()
        {
            IPacket p = null;
            switch (SocketModeReceive)
            {
                case Mode.Ascii:
                    {
                        p = new SshIdentification(pr.ReadLine().ToByteArray());
                        bytesRx += p.ToSshMessage().Length + 2;
                        break;
                    }
                case Mode.PlainText:
                    {
                        int packetLength = pr.ReadInt32();
                        byte paddingLength = pr.ReadByte();
                        var payload = pr.ReadBytes(packetLength - paddingLength - 1);
                        p = PacketFactory.Create(session, payload);
                        var padding = pr.ReadBytes(paddingLength);
                        bytesRx += packetLength + 4;

                        //using (var pw1 = new PacketWriter())
                        //{
                        //    pw1.WriteInt32(packetLength);
                        //    pw1.Write(paddingLength);
                        //    pw1.Write(payload);
                        //    pw1.Write(padding);
                        //    var raw = ((MemoryStream)pw1.BaseStream).ToArray();
                        //    Debug.WriteLine("Received Raw Packet ({0} bytes):{1}", raw.Length, raw.HexDump());
                        //}

                        break;
                    }
                case Mode.CipherText:
                    {
                        var ms = new MemoryStream();
                        var pr1 = new PacketReader(ms);
                        var pw1 = new PacketWriter(ms);

                        if (Decryptor == null)
                            decryptorSetWaitHandle.WaitOne();
                        byte[] enc = pr.ReadBytes(Decryptor.BlockSize);
                        //byte[] buffer1 = 
                        readerWaitHandle.Reset();
                        Decryptor.Transform(enc);

                        //pw1.Write(buffer1);
                        pw1.Write(enc);
                        ms.Position = 0;
                        int packetLength = pr1.ReadInt32();
                        byte paddingLength = pr1.ReadByte();

                        int bytesToRead = (packetLength + 4) - Decryptor.BlockSize;
                        if (bytesToRead > 0)
                        {
                            enc = pr.ReadBytes(bytesToRead);
                            //buffer1 = 
                            Decryptor.Transform(enc);
                            ms.Position = ms.Length;
                            pw1.Write(enc);
                            ms.Position = 5;
                        }

                        var payload = pr1.ReadBytes(packetLength - paddingLength - 1);
                        p = PacketFactory.Create(session, payload);

                        using (var hmac = DecryptorMac.Create())
                        {
                            bytesRx += packetLength + 4 + hmac.BlockSize;
                            var decrypted = ms.ToArray();
                            byte[] mac = pr.ReadBytes(hmac.BlockSize);

                            // verify mac
                            hmac.WriteUInt32(ServerSequence);
                            hmac.Write(decrypted);

                            if (!mac.SequenceEqual(hmac.Hash))
                                throw new Exception("MAC couldn't be verified!");
                        }
                        break;
                    }
            }

            if (SocketModeReceive != Mode.Ascii) ServerSequence++;
            if (p.Code == MessageCode.SSH_MSG_IDENTIFICATION)
                SocketModeReceive = Mode.PlainText;
            if (p.Code == MessageCode.SSH_MSG_NEWKEYS)
                SocketModeReceive = Mode.CipherText;

            Debug.WriteLine("Received Packet\n{0}", p);
            readerWaitHandle.Set();
            return p;
        }

        internal void WritePacket(IPacket p)
        {
            if (!socket.Connected) return;
            writerWaitHandle.WaitOne();
            readerWaitHandle.WaitOne();

            Debug.WriteLine("Sending Packet\n{0}", p);
            if (SocketModeSend == Mode.Ascii)
            {
                if (p.Code == MessageCode.SSH_MSG_IDENTIFICATION)
                {
                    var data = p.ToSshMessage().Concat(new byte[] { (byte)'\r', (byte)'\n' }).ToArray();
                    st.Write(data, 0, data.Length);
                    bytesTx += data.Length;
                    SocketModeSend = Mode.PlainText;
                }
            }
            else
            {
                byte paddingLength;
                var buffer = p.ToSshMessage();
                int length = GetPacketLength(buffer.Length, out paddingLength);

                using (var pw = new PacketWriter())
                {
                    pw.WriteInt32(length);
                    pw.Write(paddingLength);
                    pw.Write(buffer);
                    if (SocketModeSend == Mode.PlainText)
                        pw.Write(new byte[paddingLength]);
                    else
                        pw.WriteRandom(paddingLength);
                    var data = ((MemoryStream)pw.BaseStream).ToArray();

                    //Debug.WriteLine("Sent Raw Packet ({0} bytes):{1}", data.Length, data.HexDump());

                    WritePacket(data);
                    bytesTx += data.Length;
                    ClientSequence++;
                }

                if (p.Code == MessageCode.SSH_MSG_NEWKEYS)
                    SocketModeSend = Mode.CipherText;
            }

            writerWaitHandle.Set();
        }

        private void WritePacket(byte[] data)
        {
            if (SocketModeSend == Mode.CipherText)
            {
                var mac = Encrypt(data);
                st.Write(data, 0, data.Length);
                st.Write(mac, 0, mac.Length);
            }
            else
            {
                st.Write(data, 0, data.Length);
            }
        }

        private int GetPacketLength(int bufferLength, out byte paddingLength)
        {
            int blockSize = SocketModeSend == Mode.PlainText ? 8 : Encryptor.BlockSize;
            int length = bufferLength + 1;
            paddingLength = (byte)(blockSize - (length + 4) % blockSize);
            if (paddingLength < 4) paddingLength += (byte)blockSize;
            length += paddingLength;
            return length;
        }

        private byte[] Encrypt(byte[] data)
        {
            using (var hmac = EncryptorMac.Create())
            {
                hmac.WriteUInt32(ClientSequence);
                hmac.Write(data);
                Encryptor.Transform(data);
                return hmac.Hash;
            }
        }

        internal enum Mode
        {
            Ascii,
            PlainText,
            CipherText
        }
    }
}
