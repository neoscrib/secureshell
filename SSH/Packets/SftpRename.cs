namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_RENAME)]
    class SftpRename : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Source { get; set; }
        [SshProperty(5)]
        public string Target { get; set; }

        public SftpRename() : base(SftpMessageCode.SSH_FXP_RENAME) { }

        public SftpRename(uint channel, uint requestId, string source, string target)
            : base(channel, SftpMessageCode.SSH_FXP_RENAME)
        {
            this.RequestId = requestId;
            this.Source = source;
            this.Target = target;
        }
    }
}
