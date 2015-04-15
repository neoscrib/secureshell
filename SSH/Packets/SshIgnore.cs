namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_IGNORE)]
    public class SshIgnore : Packet
    {
        [SshProperty(1)]
        public byte[] Data { get; set; }

        public SshIgnore() : base(MessageCode.SSH_MSG_IGNORE) { }
    }
}
