namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_READ)]
    class SftpRead : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Handle { get; set; }
        [SshProperty(5)]
        public ulong Offset { get; set; }
        [SshProperty(6)]
        public uint Length { get; set; }

        public SftpRead() : base(SftpMessageCode.SSH_FXP_READ) { }

        public SftpRead(uint channel, uint requestId, byte[] handle, ulong offset, uint length)
            : base(channel, SftpMessageCode.SSH_FXP_READ)
        {
            this.RequestId = requestId;
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
        }
    }
}
