namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEX_DH_GEX_REQUEST)]
    public class SshDHGexRequest : Packet
    {
        [SshProperty(1)]
        public uint Min { get; set; }
        [SshProperty(2)]
        public uint N { get; set; }
        [SshProperty(3)]
        public uint Max { get; set; }

        public SshDHGexRequest()
            : this(1024, 1024, 8192)
        { }

        public SshDHGexRequest(uint min, uint n, uint max)
            : base(MessageCode.SSH_MSG_KEX_DH_GEX_REQUEST)
        {
            this.Min = min;
            this.N = n;
            this.Max = max;
        }
    }
}
