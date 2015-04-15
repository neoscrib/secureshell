namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_CLOSE)]
    public class SshChannelClose : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }

        public SshChannelClose() : base(MessageCode.SSH_MSG_CHANNEL_CLOSE) { }

        public SshChannelClose(uint channel) :
            this()
        {
            this.Channel = channel;
        }
    }
}
