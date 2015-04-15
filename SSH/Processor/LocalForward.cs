using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SSH.Packets;
using SSH.Threading;

namespace SSH.Processor
{
    public class LocalForward : ChannelProcessor
    {
        private TcpClient client;
        private Thread clientThread;
        private string remoteAddress;
        private uint remotePort;
        private IPEndPoint clientEndpoint;
        private DataWaitHandle<ISshChannelMessage> waitHandle;

        private bool channelEOFSent = false;
        private bool channelCloseSent = false;

        internal LocalForward(Session session) : base(session) { }

        public LocalForward(Session session, TcpClient client, string remoteAddress, uint remotePort)
            : base(session)
        {
            this.session = session;
            Start(client, remoteAddress, remotePort);
        }

        internal void Start(TcpClient client, string remoteAddress, uint remotePort)
        {
            this.client = client;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
            this.clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            this.waitHandle = new DataWaitHandle<ISshChannelMessage>(false, EventResetMode.AutoReset);

            try
            {
                session.Socket.WritePacket(new SshChannelOpenDirectTcpIP(this.LocalChannel,
                    this.remoteAddress, this.remotePort,
                    this.clientEndpoint.Address.ToString(), (uint)this.clientEndpoint.Port));
                waitHandle.WaitOne();

                this.clientThread = new Thread(new ThreadStart(HandleClient));
                this.clientThread.Start();
            }
            catch (SocketException ex)
            {
                Close(SSH.StatusCode.ConnectionRefused);
                throw ex;
            }
        }

        public LocalForward(Session session, TcpClient client, uint remoteChannel)
            : base(session)
        {
            this.RemoteChannel = remoteChannel;
            this.session = session;
            this.client = client;

            session.Socket.WritePacket(new SshChannelOpenConfirmation(this.RemoteChannel, this.LocalChannel, 0xffffffffU, 0x4000U));
            this.clientThread = new Thread(new ThreadStart(HandleClient));
            this.clientThread.Start();
        }

        internal void HandleClient()
        {
            try
            {
                var s = this.client.GetStream();
                var buffer = new byte[1024];
                int read = s.Read(buffer, 0, buffer.Length);
                while (read != 0)
                {
                    var data = new byte[read];
                    Buffer.BlockCopy(buffer, 0, data, 0, read);
                    session.Socket.WritePacket(new SshChannelData(this.RemoteChannel, data));
                    read = s.Read(buffer, 0, buffer.Length);
                }
                if (!this.channelCloseSent)
                {
                    this.channelEOFSent = true;
                    this.session.Socket.WritePacket(new SshChannelEOF(this.RemoteChannel));
                }
            }
            catch (InvalidOperationException) { }
            catch (IOException) { }
        }

        public override void Close()
        {
            this.client.Close();
            base.Close();
        }

        public override bool InternalProcessPacket(ISshChannelMessage p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION:
                    {
                        var msg = (SshChannelOpenConfirmation)p;
                        this.RemoteChannel = msg.SenderChannel;
                        waitHandle.Set();
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_OPEN_FAILURE:
                    {
                        var msg = (SshChannelOpenFailure)p;
                        waitHandle.Set(new SocketException(
                            msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_ADMINISTRATIVELY_PROHIBITED ? 10013 :
                            msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_CONNECT_FAILED ? 10061 :
                            msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_UNKNOWN_CHANNEL_TYPE ? 10043 :
                            msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_RESOURCE_SHORTAGE ? 8 : 0));
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_WINDOW_ADJUST:
                    return true;
                case MessageCode.SSH_MSG_CHANNEL_DATA:
                    {
                        var msg = (SshChannelData)p;
                        try
                        {
                            if (client.Connected)
                            {
                                var s = client.GetStream();
                                if (s.CanWrite)
                                    s.Write(msg.Data, 0, msg.Data.Length);
                                else throw new IOException();
                            }
                            else throw new IOException();
                        }
                        catch (IOException)
                        {
                            if (!this.channelCloseSent)
                            {
                                this.channelCloseSent = true;
                                session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
                            }
                        }
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_EOF:
                    {
                        var msg = (SshChannelEOF)p;
                        if (!this.channelEOFSent)
                        {
                            this.channelCloseSent = true;
                            session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
                        }
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_CLOSE:
                    {
                        this.client.Close();
                        if (!this.channelCloseSent)
                        {
                            this.channelCloseSent = true;
                            session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
                        }
                        Close(SSH.StatusCode.OK);
                        return true;
                    }
            }
            return false;
        }
    }
}
