using SSH.Collections;
using SSH.Compression;
using SSH.Encryption;
using SSH.IO;
using SSH.Packets;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace SSH
{
    class SshSocket : IDisposable
    {
        TcpClient socket;
        MetricsStream st;
        PacketWriter pw;
        PacketReader pr;

        Thread reader;
        Session session;
        Thread receiveProcessor;
        Thread transmitProcessor;
        BlockingQueue<IPacket> receiveQueue;
        BlockingQueue<IPacket> transmitQueue;

        Cipher decryptor;

        internal Mode SocketModeRx { get; set; }
        internal Mode SocketModeTx { get; set; }
        internal MacWriter EncryptorMac { get; set; }
        internal MacWriter DecryptorMac { get; set; }
        internal uint ServerSequence { get; set; }
        internal uint ClientSequence { get; set; }

        internal Cipher Encryptor { get; set; }
        internal Cipher Decryptor
        {
            get { return decryptor; }
            set
            {
                decryptor = value;
                pr = new PacketReader(new CryptoStream(st, decryptor, CryptoStreamMode.Read));
            }
        }

        private ZlibTransform RxZlibTransform { get; set; }
        private ZlibTransform TxZlibTransform { get; set; }

        byte[] zlibRxBuffer = new byte[32768];
        byte[] zlibTxBuffer = new byte[32768];
        bool rxCompressionEnabled = false;
        bool txCompressionEnabled = false;

        internal SshSocket(Session session)
        {
            this.session = session;
            this.SocketModeRx = Mode.Ascii;
            this.SocketModeTx = Mode.Ascii;
            this.socket = new TcpClient();
        }

        internal StatusCode Connect(string hostname, int port)
        {
            var async = socket.BeginConnect(hostname, port, null, null);
            var success = async.AsyncWaitHandle.WaitOne(2000, true);
            if (!success) return SSH.StatusCode.ConnectionTimedOut;
            if (!socket.Connected) return SSH.StatusCode.ConnectionRefused;

            st = new MetricsStream(socket.GetStream());
            pr = new PacketReader(st);
            pw = new PacketWriter(st);

            receiveQueue = new BlockingQueue<IPacket>();
            receiveProcessor = new Thread(new ThreadStart(ProcessReceiveQueue));
            receiveProcessor.Start();
            transmitQueue = new BlockingQueue<IPacket>();
            transmitProcessor = new Thread(new ThreadStart(ProcessTransmitQueue));
            transmitProcessor.Start();
            reader = new Thread(new ThreadStart(ReadPackets));
            reader.Start();

            return StatusCode.OK;
        }

        internal void Disconnect()
        {
            foreach (var t in new Thread[] { reader, transmitProcessor, receiveProcessor })
            {
                try
                {
                    t.Abort();
                    t.Join();
                }
                catch (NullReferenceException) { }
            }

            socket.Close();
            Debug.WriteLine("Transferred: Sent {0}; Received {1}",
                Util.ToHumanReadableSize(st.BytesWritten), Util.ToHumanReadableSize(st.BytesRead));
        }

        private void ProcessTransmitQueue()
        {
            try
            {
                while (true)
                {
                    var p = transmitQueue.Dequeue();
                    Debug.WriteLine("Sending Packet\n{0}", p);
                    if (p != null) InternalWritePacket(p);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("Transmit queue processor thread aborting.");
                Thread.ResetAbort();
            }
        }

        private void ProcessReceiveQueue()
        {
            try
            {
                while (true)
                {
                    var p = receiveQueue.Dequeue();
                    Debug.WriteLine("Received Packet\n{0}", p);
                    if (p != null) session.Processors.Process(p);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("Receive queue processor thread aborting.");
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
                    receiveQueue.Enqueue(p);
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
            switch (SocketModeRx)
            {
                case Mode.Ascii:
                    {
                        p = new SshIdentification(pr.ReadLine().ToByteArray());
                        break;
                    }
                case Mode.PlainText:
                    {
                        ServerSequence++;
                        int packetLength = pr.ReadInt32();
                        byte paddingLength = pr.ReadByte();
                        var payload = pr.ReadBytes(packetLength - paddingLength - 1);
                        pr.ReadBytes(paddingLength);
                        p = PacketFactory.Create(session, payload);
                        break;
                    }
                case Mode.CipherText:
                    {
                        if (Decryptor == null)
                            session.KexProcessor.Wait();

                        int packetLength = pr.ReadInt32();
                        byte paddingLength = pr.ReadByte();
                        var payload = pr.ReadBytes(packetLength - paddingLength - 1);
                        var padding = pr.ReadBytes(paddingLength);

                        var hmac = DecryptorMac;
                        hmac.WriteUInt32(ServerSequence++);
                        hmac.WriteInt32(packetLength);
                        hmac.Write(paddingLength);
                        hmac.Write(payload);
                        hmac.Write(padding);

                        if (rxCompressionEnabled || (rxCompressionEnabled = IsRxCompressionEnabled()))
                        {
                            if (RxZlibTransform == null)
                                RxZlibTransform = new ZlibTransform(CompressionMode.Decompress);

                            var length = RxZlibTransform.TransformBlock(payload, 0, payload.Length, zlibRxBuffer, 0);
                            Debug.WriteLine(string.Format("Compression In: In: {0}; Out: {1}; TotalIn: {2}; TotalOut: {3}; TotalRatio: {4}",
                                payload.Length, length, RxZlibTransform.TotalIn, RxZlibTransform.TotalOut, RxZlibTransform.Ratio));
                            payload = new byte[length];
                            Buffer.BlockCopy(zlibRxBuffer, 0, payload, 0, length);
                        }

                        // mac isn't encrypted, read directly from network stream
                        byte[] mac = new byte[hmac.BlockSize];
                        st.Read(mac, 0, hmac.BlockSize);

                        if (!hmac.IsMatch(mac))
                            throw new CryptographicException("MAC couldn't be verified!");
                        hmac.Reset();

                        p = PacketFactory.Create(session, payload);
                        break;
                    }
            }

            if (p.Code == MessageCode.SSH_MSG_IDENTIFICATION)
                SocketModeRx = Mode.PlainText;
            if (p.Code == MessageCode.SSH_MSG_NEWKEYS)
                SocketModeRx = Mode.CipherText;
            return p;
        }

        internal void WritePacket(IPacket p)
        {
            transmitQueue.Enqueue(p);
        }

        private void InternalWritePacket(IPacket p)
        {
            if (!socket.Connected) return;

            byte paddingLength;
            var payload = p.ToSshMessage();

            switch (SocketModeTx)
            {
                case Mode.Ascii:
                    {
                        var data = payload.Concat(new byte[] { (byte)'\r', (byte)'\n' }).ToArray();
                        st.Write(data, 0, data.Length);
                        SocketModeTx = Mode.PlainText;
                        break;
                    }
                case Mode.PlainText:
                    {
                        ClientSequence++;
                        int packetLength = GetPacketLength(payload.Length, out paddingLength);
                        pw.WriteInt32(packetLength);
                        pw.Write(paddingLength);
                        pw.Write(payload);
                        pw.Write(new byte[paddingLength]);
                        break;
                    }
                case Mode.CipherText:
                    {
                        if (txCompressionEnabled || (txCompressionEnabled = IsTxCompressionEnabled()))
                        {
                            if (TxZlibTransform == null)
                                TxZlibTransform = new ZlibTransform(CompressionMode.Compress);

                            var compressedLength = TxZlibTransform.TransformBlock(payload, 0, payload.Length, zlibTxBuffer, 0);
                            Debug.WriteLine(string.Format("Compression Out: In: {0}; Out: {1}; TotalIn: {2}; TotalOut: {3}; TotalRatio: {4}",
                                payload.Length, compressedLength, TxZlibTransform.TotalIn, TxZlibTransform.TotalOut, TxZlibTransform.Ratio));
                            payload = new byte[compressedLength];
                            Buffer.BlockCopy(zlibTxBuffer, 0, payload, 0, compressedLength);
                        }

                        var hmac = EncryptorMac;
                        int packetLength = GetPacketLength(payload.Length, out paddingLength);
                        byte[] padding = new byte[paddingLength];
                        Extensions.Random.GetBytes(padding);

                        hmac.WriteUInt32(ClientSequence++);
                        hmac.WriteInt32(packetLength);
                        hmac.Write(paddingLength);
                        hmac.Write(payload);
                        hmac.Write(padding);

                        pw.WriteInt32(packetLength);
                        pw.Write(paddingLength);
                        pw.Write(payload);
                        pw.Write(padding);
                        // mac isn't encrypted, write directly to network stream
                        st.Write(hmac.Hash, 0, hmac.BlockSize);
                        hmac.Reset();
                        break;
                    }
            }

            if (p.Code == MessageCode.SSH_MSG_NEWKEYS)
            {
                SocketModeTx = Mode.CipherText;
                pw = new PacketWriter(new CryptoStream(st, Encryptor, CryptoStreamMode.Write));
            }
        }

        private int GetPacketLength(int bufferLength, out byte paddingLength)
        {
            int blockSize = SocketModeTx == Mode.PlainText ? 8 : Encryptor.BlockSize;
            int length = bufferLength + 1;
            paddingLength = (byte)(blockSize - (length + 4) % blockSize);
            if (paddingLength < 4) paddingLength += (byte)blockSize;
            length += paddingLength;
            return length;
        }

        private bool IsTxCompressionEnabled()
        {
            return (session.Algorithms.ComAlgorithmsClient.First().Equals("zlib") && SocketModeTx == Mode.CipherText) ||
                (session.Algorithms.ComAlgorithmsClient.First().Equals("zlib@openssh.com") && session.IsAuthenticated);
        }

        private bool IsRxCompressionEnabled()
        {
            return (session.Algorithms.ComAlgorithmsServer.First().Equals("zlib") && SocketModeRx == Mode.CipherText) ||
                (session.Algorithms.ComAlgorithmsServer.First().Equals("zlib@openssh.com") && session.IsAuthenticated);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                pr.Close();
                pw.Close();
                st.Close();
                socket.Close();
                receiveQueue.Dispose();
                transmitQueue.Dispose();
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
