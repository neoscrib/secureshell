namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEX_DH_GEX_INIT)]
    public class SshDHGexInit : Packet
    {
        [SshProperty(1)]
        public byte[] E { get; set; }

        public SshDHGexInit() : base(MessageCode.SSH_MSG_KEX_DH_GEX_INIT) { }

        public SshDHGexInit(byte[] e)
            : this()
        {
            this.E = e;
        }
    }
}
