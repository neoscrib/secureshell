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

        public abstract bool InternalProcessPacket(ISshChannelMessage p);

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
                    return InternalProcessPacket(msg);
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
