namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_STATUS)]
    class SftpStatus : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public SftpStatusCode Status { get; set; }
        [SshProperty(5)]
        public string Message { get; set; }
        [SshProperty(6)]
        public string LanguageCode { get; set; }

        public SftpStatus() : base(SftpMessageCode.SSH_FXP_STATUS) { }
    }
}
