namespace SSH.Packets
{
    [Packet(Packets.MessageCode.SSH_MSG_CHANNEL_OPEN_FAILURE)]
    public class SshChannelOpenFailure : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public ChannelOpenFailureReasonCode ReasonCode { get; set; }
        [SshProperty(3)]
        public string Description { get; set; }
        [SshProperty(4)]
        public string Language { get; set; }

        public SshChannelOpenFailure() : base(MessageCode.SSH_MSG_CHANNEL_OPEN_FAILURE) { }

        public enum ChannelOpenFailureReasonCode : uint
        {
            SSH_OPEN_ADMINISTRATIVELY_PROHIBITED = 1,
            SSH_OPEN_CONNECT_FAILED = 2,
            SSH_OPEN_UNKNOWN_CHANNEL_TYPE = 3,
            SSH_OPEN_RESOURCE_SHORTAGE = 4
        }
    }
}
