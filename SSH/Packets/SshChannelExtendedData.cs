namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_EXTENDED_DATA)]
    public class SshChannelExtendedData : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public DataTypeCode DataType { get; set; }
        [SshProperty(3)]
        public byte[] Data { get; set; }

        public SshChannelExtendedData() : base(MessageCode.SSH_MSG_CHANNEL_EXTENDED_DATA) { }

        public SshChannelExtendedData(uint channel, DataTypeCode dataType, byte[] data)
            : this()
        {
            this.Channel = channel;
            this.DataType = dataType;
            this.Data = data;
        }

        public enum DataTypeCode : uint
        {
            SSH_EXTENDED_DATA_STDERR = 1
        }
    }
}
