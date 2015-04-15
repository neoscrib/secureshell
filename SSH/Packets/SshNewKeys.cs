namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_NEWKEYS)]
    public class SshNewKeys : Packet
    {
        public SshNewKeys() : base(MessageCode.SSH_MSG_NEWKEYS) { }
    }
}
