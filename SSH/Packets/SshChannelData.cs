namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_DATA)]
    public class SshChannelData : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public byte[] Data { get; set; }

        public SshChannelData() : base(MessageCode.SSH_MSG_CHANNEL_DATA) { }

        public SshChannelData(uint channel, byte[] data)
            : this()
        {
            Channel = channel;
            Data = data;
        }
    }
}
