namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_ATTRS)]
    class SftpAttrs : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public SftpFileInfo Attrs { get; set; }

        public SftpAttrs() : base(SftpMessageCode.SSH_FXP_ATTRS) { }
    }
}
