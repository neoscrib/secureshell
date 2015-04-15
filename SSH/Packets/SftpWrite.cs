namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_WRITE)]
    class SftpWrite : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Handle { get; set; }
        [SshProperty(5)]
        public ulong Position { get; set; }
        [SshProperty(6)]
        public byte[] Data { get; set; }

        public SftpWrite() : base(SftpMessageCode.SSH_FXP_WRITE) { }

        public SftpWrite(uint channel, uint requestId, byte[] handle, ulong position, byte[] data)
            : base(channel, SftpMessageCode.SSH_FXP_WRITE)
        {
            this.RequestId = requestId;
            this.Handle = handle;
            this.Position = position;
            this.Data = data;
        }
    }
}
