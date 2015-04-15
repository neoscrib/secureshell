namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_HANDLE)]
    class SftpHandle : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public byte[] Handle { get; set; }

        public SftpHandle() : base(SftpMessageCode.SSH_FXP_HANDLE) { }
    }
}
