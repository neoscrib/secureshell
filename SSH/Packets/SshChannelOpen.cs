namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_OPEN)]
    public class SshChannelOpen : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public string ChannelType { get; set; }
        [SshProperty(2)]
        public uint Channel { get; set; }
        [SshProperty(3)]
        public uint WindowSize { get; set; }
        [SshProperty(4)]
        public uint PacketSize { get; set; }

        public SshChannelOpen() : base(MessageCode.SSH_MSG_CHANNEL_OPEN) { }

        public SshChannelOpen(string type, uint channel, uint windowSize, uint packetSize)
            : this()
        {
            this.ChannelType = type;
            this.Channel = channel;
            this.WindowSize = windowSize;
            this.PacketSize = packetSize;
        }
    }
}
