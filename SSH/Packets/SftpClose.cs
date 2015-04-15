namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_CLOSE)]
    class SftpClose : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Handle { get; set; }

        public SftpClose() : base(SftpMessageCode.SSH_FXP_CLOSE) { }

        public SftpClose(uint channel, uint requestId, byte[] handle)
            : base(channel, SftpMessageCode.SSH_FXP_CLOSE)
        {
            this.RequestId = requestId;
            this.Handle = handle;
        }
    }
}
