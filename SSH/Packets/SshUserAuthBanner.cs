namespace SSH.Packets
{
    [Packet(Packets.MessageCode.SSH_MSG_USERAUTH_BANNER)]
    public class SshUserAuthBanner : Packet
    {
        [SshProperty(1)]
        public string Message { get; set; }
        [SshProperty(2)]
        public string Language { get; set; }

        public SshUserAuthBanner()
            : base(MessageCode.SSH_MSG_USERAUTH_BANNER)
        { }
    }
}
