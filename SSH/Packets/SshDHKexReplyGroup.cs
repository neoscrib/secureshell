namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEXDH_REPLY)]
    public class SshDHKexReplyGroup : Packet
    {
        [SshProperty(1)]
        public byte[] K { get; set; }
        [SshProperty(2)]
        public byte[] F { get; set; }
        [SshProperty(3)]
        public byte[] S { get; set; }

        public SshDHKexReplyGroup() : base(MessageCode.SSH_MSG_KEXDH_REPLY) { }
    }
}
