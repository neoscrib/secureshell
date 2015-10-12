using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SSH.Processor
{
    public class SocksLocalForward : LocalForward
    {
        Thread socksThread;
        TcpClient client;

        public SocksLocalForward(Session session, TcpClient client)
            : base(session)
        {
            this.client = client;
            socksThread = new Thread(new ThreadStart(DoSocks));
            socksThread.Start();
        }

        private void DoSocks()
        {
            var s = client.GetStream();
            var version = (byte)s.ReadByte();

            if (version == 5)
            {
                var nmethods = (byte)s.ReadByte();
                var methods = new List<byte>();
                for (int i = 0; i < nmethods; i++)
                    methods.Add((byte)s.ReadByte());
                if (version == 5 && methods.Contains(0))
                    s.Write(new byte[] { 0x05, 0x00 }, 0, 2);
                version = (byte)s.ReadByte();
            }

            var command = (byte)s.ReadByte();
            var destination = string.Empty;
            var port = (ushort)0;

            if (version == 5)
            {
                s.ReadByte(); // reserved
                var atype = (byte)s.ReadByte();
                destination = string.Empty;
                switch (atype)
                {
                    case 1:
                        {
                            var buffer = new byte[4];
                            s.Read(buffer, 0, buffer.Length);
                            destination = new IPAddress(buffer).ToString();
                            break;
                        }
                    case 3:
                        {
                            var length = (byte)s.ReadByte();
                            var buffer = new byte[length];
                            s.Read(buffer, 0, buffer.Length);
                            destination = System.Text.Encoding.Default.GetString(buffer);
                            break;
                        }
                    case 4:
                        {
                            var buffer = new byte[16];
                            s.Read(buffer, 0, buffer.Length);
                            destination = new IPAddress(buffer).ToString();
                            break;
                        }
                }
                port = (ushort)(s.ReadByte() << 8 | s.ReadByte());
            }
            else if (version == 4)
            {
                port = (ushort)(s.ReadByte() << 8 | s.ReadByte());
                var buffer = new byte[4];
                s.Read(buffer, 0, buffer.Length);
                destination = new IPAddress(buffer).ToString();
                while (s.ReadByte() != 0) ;
                if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0 && buffer[3] != 0) // version 4a
                {
                    destination = string.Empty;
                    byte b;
                    while ((b = (byte)s.ReadByte()) != 0)
                        destination += (char)b;
                }
            }
            else
            {
                FailFast(client, s, 0x01);
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                try
                {
                    var bind = (IPEndPoint)client.Client.RemoteEndPoint;
                    byte[] data = new byte[8];
                    if (version == 5)
                    {
                        data = new byte[6 + bind.Address.GetAddressBytes().Length];
                        data[0] = 0x05;
                        data[1] = 0x00;
                        data[2] = 0x00;
                        data[3] = bind.Address.GetAddressBytes().Length == 4 ? (byte)0x01 : (byte)0x04;
                        Array.Copy(bind.Address.GetAddressBytes(), 0, data, 4, bind.Address.GetAddressBytes().Length);
                        data[data.Length - 2] = (byte)(bind.Port >> 8);
                        data[data.Length - 1] = (byte)bind.Port;
                    }
                    else
                    {
                        data[0] = 0x00;
                        data[1] = 0x5a;
                        data[2] = (byte)(bind.Port >> 8);
                        data[3] = (byte)bind.Port;
                        Array.Copy(bind.Address.GetAddressBytes(), 0, data, 4, 4);
                    }
                    s.Write(data, 0, data.Length);
                    Start(client, destination, (uint)port);
                }
                catch (SocketException ex)
                {
                    FailFast(client, s, (byte)(ex.ErrorCode == 10013 ? 0x02 : ex.ErrorCode == 10061 ? 0x05 : 0x01));
                }
            }
            else
            {
                FailFast(client, s, 0x01);
            }
        }

        private static void FailFast(TcpClient client, NetworkStream s, byte errorCode)
        {
            var data = new byte[] { 0x05, errorCode, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            s.Write(data, 0, data.Length);
            client.Close();
        }
    }
}
