namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_SUCCESS)]
    public class SshChannelSuccess : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }

        public SshChannelSuccess() : base(MessageCode.SSH_MSG_CHANNEL_SUCCESS) { }

        public SshChannelSuccess(uint channel)
            : this()
        {
            this.Channel = channel;
        }
    }
}
