using SSH.Packets;
using SSH.Threading;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SSH.Processor
{
    public class LocalForward : ChannelProcessor
    {
        private TcpClient client;
        private string remoteAddress;
        private uint remotePort;
        private IPEndPoint clientEndpoint;
        private DataWaitHandle<ISshChannelMessage> waitHandle;

        private bool channelEOFSent = false;
        private bool channelCloseSent = false;

        private byte[] clientBuffer = new byte[0x4000];

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

                this.client.GetStream().BeginRead(clientBuffer, 0, clientBuffer.Length, HandleClient, null);
            }
            catch (SocketException)
            {
                Close(StatusCode.ConnectionRefused);
                throw;
            }
        }

        public LocalForward(Session session, TcpClient client, uint remoteChannel)
            : base(session)
        {
            this.RemoteChannel = remoteChannel;
            this.session = session;
            this.client = client;

            session.Socket.WritePacket(new SshChannelOpenConfirmation(this.RemoteChannel, this.LocalChannel, 0xffffffffU, 0x4000U));
            client.GetStream().BeginRead(clientBuffer, 0, clientBuffer.Length, HandleClient, null);
        }

        internal void HandleClient(IAsyncResult result)
        {
            try
            {
                int read = 0;
                if (this.client.Connected && (read = this.client.GetStream().EndRead(result)) > 0)
                {
                    var data = new byte[read];
                    Buffer.BlockCopy(clientBuffer, 0, data, 0, read);
                    session.Socket.WritePacket(new SshChannelData(this.RemoteChannel, data));
                    client.GetStream().BeginRead(clientBuffer, 0, clientBuffer.Length, new AsyncCallback(HandleClient), null);
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            finally
            {
                if (!this.channelEOFSent)
                {
                    this.channelEOFSent = true;
                    this.session.Socket.WritePacket(new SshChannelEOF(this.RemoteChannel));
                }
            }
        }

        public override void Close()
        {
            if (this.client != null)
                this.client.Close();
            base.Close();
        }

        public override void OnChannelOpenConfirmation(ISshChannelMessage p)
        {
            var msg = (SshChannelOpenConfirmation)p;
            this.RemoteChannel = msg.SenderChannel;
            waitHandle.Set();
        }

        public override void OnChannelOpenFailure(ISshChannelMessage p)
        {
            var msg = (SshChannelOpenFailure)p;
            waitHandle.Set(new SocketException(
                msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_ADMINISTRATIVELY_PROHIBITED ? 10013 :
                msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_CONNECT_FAILED ? 10061 :
                msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_UNKNOWN_CHANNEL_TYPE ? 10043 :
                msg.ReasonCode == SshChannelOpenFailure.ChannelOpenFailureReasonCode.SSH_OPEN_RESOURCE_SHORTAGE ? 8 : 0));
        }

        public override void OnChannelSuccess(ISshChannelMessage p) { }

        public override void OnChannelWindowAdjust(ISshChannelMessage p) { }

        public override void OnChannelData(ISshChannelMessage p)
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
        }

        public override void OnChannelExtendedData(ISshChannelMessage p) { }

        public override void OnChannelClose(ISshChannelMessage p)
        {
            this.client.Close();
            if (!this.channelCloseSent)
            {
                this.channelCloseSent = true;
                session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
            }
            Close(SSH.StatusCode.OK);
        }

        public override void OnChannelEndOfFile(ISshChannelMessage p)
        {
            var msg = (SshChannelEOF)p;
            if (!this.channelEOFSent)
            {
                this.channelEOFSent = true;
                session.Socket.WritePacket(new SshChannelEOF(this.RemoteChannel));
            }
        }

        public override void OnChannelRequest(ISshChannelMessage p) { }
    }
}
