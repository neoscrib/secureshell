namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_READDIR)]
    class SftpReadDirectory : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Handle { get; set; }

        public SftpReadDirectory() : base(SftpMessageCode.SSH_FXP_READDIR) { }

        public SftpReadDirectory(uint channel, uint requestId, byte[] handle)
            : base(channel, SftpMessageCode.SSH_FXP_READDIR)
        {
            this.RequestId = requestId;
            this.Handle = handle;
        }
    }
}
