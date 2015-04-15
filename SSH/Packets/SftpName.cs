namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_NAME)]
    class SftpName : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public uint Count { get; set; }
        [SshProperty(5)]
        public SftpFileInfo[] Files { get; set; }

        public SftpName() : base(SftpMessageCode.SSH_FXP_NAME) { }
    }
}
