namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEX_ECDH_REPLY)]
    public class SshECDHKexReply : Packet
    {
        [SshProperty(1)]
        public byte[] K { get; set; }
        [SshProperty(2)]
        public byte[] Q { get; set; }
        [SshProperty(3)]
        public byte[] S { get; set; }

        public SshECDHKexReply() : base(MessageCode.SSH_MSG_KEX_ECDH_REPLY) { }
    }
}
