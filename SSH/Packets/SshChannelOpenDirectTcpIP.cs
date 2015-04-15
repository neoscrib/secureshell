namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_OPEN)]
    class SshChannelOpenDirectTcpIP : SshChannelOpen
    {
        [SshProperty(5)]
        public string RemoteAddress { get; set; }
        [SshProperty(6)]
        public uint RemotePort { get; set; }
        [SshProperty(7)]
        public string ClientAddress { get; set; }
        [SshProperty(8)]
        public uint ClientPort { get; set; }

        public SshChannelOpenDirectTcpIP() : base() { }

        public SshChannelOpenDirectTcpIP(uint channel, string remoteAddr,
            uint remotePort, string clientAddr, uint clientPort)
            : base("direct-tcpip", channel, 0xffffffff, 4096U)
        {
            this.RemoteAddress = remoteAddr;
            this.RemotePort = remotePort;
            this.ClientAddress = clientAddr;
            this.ClientPort = clientPort;
        }
    }
}
