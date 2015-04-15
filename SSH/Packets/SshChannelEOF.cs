namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_EOF)]
    public class SshChannelEOF : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }

        public SshChannelEOF() : base(MessageCode.SSH_MSG_CHANNEL_EOF) { }

        public SshChannelEOF(uint channel)
            : this()
        {
            this.Channel = channel;
        }
    }
}
