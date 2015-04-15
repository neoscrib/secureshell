namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_MKDIR)]
    class SftpMakeDir : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }
        [SshProperty(5)]
        public SftpFileAttributes Attributes { get; set; }

        public SftpMakeDir() : base(SftpMessageCode.SSH_FXP_MKDIR) { }

        public SftpMakeDir(uint channel, uint requestId, string path, string permissions = "")
            : base(channel, SftpMessageCode.SSH_FXP_MKDIR)
        {
            this.RequestId = requestId;
            this.Path = path;
            this.Attributes = SftpFileAttributes.ParsePermissions(permissions);
        }
    }
}
