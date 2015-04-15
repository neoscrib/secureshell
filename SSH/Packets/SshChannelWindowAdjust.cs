namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_WINDOW_ADJUST)]
    public class SshChannelWindowAdjust : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public uint BytesToAdd { get; set; }

        public SshChannelWindowAdjust() : base(MessageCode.SSH_MSG_CHANNEL_WINDOW_ADJUST) { }
    }
}
