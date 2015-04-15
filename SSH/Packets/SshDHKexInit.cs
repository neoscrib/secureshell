namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEXDH_INIT)]
    public class SshDHKexInit : Packet
    {
        [SshProperty(1)]
        public byte[] E { get; set; }

        public SshDHKexInit() : base(MessageCode.SSH_MSG_KEXDH_INIT) { }

        public SshDHKexInit(byte[] e)
            : this()
        {
            this.E = e;
        }
    }
}
