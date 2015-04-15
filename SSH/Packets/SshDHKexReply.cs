namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEXDH_REPLY)]
    public class SshDHKexReply : Packet
    {
        [SshProperty(1)]
        public byte[] P { get; set; }
        [SshProperty(2)]
        public byte[] G { get; set; }

        public SshDHKexReply() : base(MessageCode.SSH_MSG_KEXDH_REPLY) { }
    }
}
