namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_OPENDIR)]
    class SftpOpenDirectory : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }

        public SftpOpenDirectory() : base(SftpMessageCode.SSH_FXP_OPENDIR) { }

        public SftpOpenDirectory(uint channel, uint requestId, string path) :
            base(channel, SftpMessageCode.SSH_FXP_OPENDIR)
        {
            this.RequestId = requestId;
            this.Path = path;
        }
    }
}
