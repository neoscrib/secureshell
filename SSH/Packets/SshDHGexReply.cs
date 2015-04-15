namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEX_DH_GEX_REPLY)]
    public class SshDHGexReply : Packet
    {
        [SshProperty(1)]
        public byte[] K { get; set; }
        [SshProperty(2)]
        public byte[] F { get; set; }
        [SshProperty(3)]
        public byte[] S { get; set; }

        public SshDHGexReply() : base(MessageCode.SSH_MSG_KEX_DH_GEX_REPLY) { }
    }
}
