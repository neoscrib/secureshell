namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_REMOVE)]
    class SftpRemove : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }

        public SftpRemove() : base(SftpMessageCode.SSH_FXP_REMOVE) { }

        public SftpRemove(uint channel, uint requestId, string path)
            : base(channel, SftpMessageCode.SSH_FXP_REMOVE)
        {
            this.RequestId = requestId;
            this.Path = path;
        }
    }
}
