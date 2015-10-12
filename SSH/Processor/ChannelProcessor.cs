using SSH.Packets;

namespace SSH.Processor
{
    public abstract class ChannelProcessor : PacketProcessor, IChannelProcessor
    {
        public uint LocalChannel { get; set; }
        public uint RemoteChannel { get; set; }
        public ChannelType Type { get; set; }
        public uint ExitCode { get; set; }

        public ChannelProcessor(Session session)
            : base(session)
        {
            LocalChannel = session.NextChannel++;
        }

        //public abstract bool InternalProcessPacket(ISshChannelMessage p);
        public abstract void OnChannelOpenConfirmation(ISshChannelMessage p);
        public abstract void OnChannelOpenFailure(ISshChannelMessage p);
        public abstract void OnChannelSuccess(ISshChannelMessage p);
        public abstract void OnChannelWindowAdjust(ISshChannelMessage p);
        public abstract void OnChannelData(ISshChannelMessage p);
        public abstract void OnChannelExtendedData(ISshChannelMessage p);
        public abstract void OnChannelClose(ISshChannelMessage p);
        public abstract void OnChannelEndOfFile(ISshChannelMessage p);
        public abstract void OnChannelRequest(ISshChannelMessage p);

        public override bool InternalProcessPacket(IPacket p)
        {
            if (p is ISshChannelMessage)
            {
                var msg = (ISshChannelMessage)p;
                if (msg.Channel == this.LocalChannel)
                {
                    if (p is SshChannelRequestKeepAlive)
                    {
                        session.Socket.WritePacket(new SshChannelSuccess(this.RemoteChannel));
                        return true;
                    }
                    else if (p is SshChannelRequestExitStatus)
                    {
                        this.ExitCode = ((SshChannelRequestExitStatus)p).ExitStatus;
                        return true;
                    }




                    switch (p.Code)
                    {
                        case MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION:
                            OnChannelOpenConfirmation(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_OPEN_FAILURE:
                            OnChannelOpenFailure(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_SUCCESS:
                            OnChannelSuccess(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_WINDOW_ADJUST:
                            OnChannelWindowAdjust(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_DATA:
                            OnChannelData(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_EXTENDED_DATA:
                            OnChannelExtendedData(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_CLOSE:
                            OnChannelClose(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_EOF:
                            OnChannelEndOfFile(msg);
                            return true;
                        case MessageCode.SSH_MSG_CHANNEL_REQUEST:
                            OnChannelRequest(msg);
                            return true;
                    }
                    return false;



                }
            }
            return false;
        }

        public enum ChannelType
        {
            Shell,
            Exec,
            Sftp,
            Other
        }
    }
}
