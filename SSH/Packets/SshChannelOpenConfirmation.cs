namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION)]
    public class SshChannelOpenConfirmation : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public uint SenderChannel { get; set; }
        [SshProperty(3)]
        public uint WindowsSize { get; set; }
        [SshProperty(4)]
        public uint PacketSize { get; set; }

        public SshChannelOpenConfirmation() : base(MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION) { }

        public SshChannelOpenConfirmation(uint channel, uint localChannel, uint windowSize, uint packetSize)
            : base(MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION)
        {
            this.Channel = channel;
            this.SenderChannel = localChannel;
            this.WindowsSize = windowSize;
            this.PacketSize = packetSize;
        }
    }
}
