using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SSH.Packets;
using System.Threading;
using System.Net;

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
            var nmethods = (byte)s.ReadByte();
            var methods = new List<byte>();
            for (int i = 0; i < nmethods; i++)
                methods.Add((byte)s.ReadByte());
            if (version == 5 && methods.Contains(0))
                s.Write(new byte[] { 0x05, 0x00 }, 0, 2);

            version = (byte)s.ReadByte();
            var command = (byte)s.ReadByte();
            s.ReadByte();
            var atype = (byte)s.ReadByte();
            string destination = string.Empty;
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
            var port = (ushort)(s.ReadByte() << 8 | s.ReadByte());

            if (!string.IsNullOrWhiteSpace(destination))
            {
                try
                {
                    var bind = (IPEndPoint)client.Client.RemoteEndPoint;
                    var data = new byte[6 + bind.Address.GetAddressBytes().Length];
                    data[0] = 0x05;
                    data[1] = 0x00;
                    data[2] = 0x00;
                    data[3] = bind.Address.GetAddressBytes().Length == 4 ? (byte)0x01 : (byte)0x04;
                    Array.Copy(bind.Address.GetAddressBytes(), 0, data, 4, bind.Address.GetAddressBytes().Length);
                    data[data.Length - 2] = (byte)(bind.Port >> 8);
                    data[data.Length - 1] = (byte)bind.Port;
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
            var data = new byte[] { 0x05, errorCode, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            s.Write(data, 0, data.Length);
            client.Close();
        }
    }
}
