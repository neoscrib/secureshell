namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_STAT)]
    class SftpStat : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }

        public SftpStat() : base(SftpMessageCode.SSH_FXP_STAT) { }

        public SftpStat(uint channel, uint requestId, string path)
            : base(channel, SftpMessageCode.SSH_FXP_STAT)
        {
            this.RequestId = requestId;
            this.Path = path;
        }
    }
}
