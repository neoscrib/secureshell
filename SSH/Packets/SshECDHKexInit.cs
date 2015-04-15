namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEX_ECDH_INIT)]
    public class SshECDHKexInit : Packet
    {
        [SshProperty(1)]
        public byte[] Q { get; set; }

        public SshECDHKexInit() : base(MessageCode.SSH_MSG_KEX_ECDH_INIT) { }

        public SshECDHKexInit(byte[] q)
            : this()
        {
            this.Q = q;
        }
    }
}
