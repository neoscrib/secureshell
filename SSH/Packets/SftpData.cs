namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_DATA)]
    class SftpData : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Data { get; set; }

        public SftpData() : base(SftpMessageCode.SSH_FXP_DATA) { }
    }
}
