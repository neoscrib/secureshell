namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_RMDIR)]
    class SftpRemoveDir : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }

        public SftpRemoveDir() : base(SftpMessageCode.SSH_FXP_RMDIR) { }

        public SftpRemoveDir(uint channel, uint requestId, string path)
            : base(channel, SftpMessageCode.SSH_FXP_RMDIR)
        {
            this.RequestId = requestId;
            this.Path = path;
        }
    }
}
