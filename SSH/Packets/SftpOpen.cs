using System;

namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_OPEN)]
    class SftpOpen : SftpPacket
    {
        [SshProperty(3)]
        public uint RequestId { get; set; }
        [SshProperty(4)]
        public string Path { get; set; }
        [SshProperty(5)]
        public FileModeFlags FileMode { get; set; }
        [SshProperty(6)]
        public SftpFileAttributes Attributes { get; set; }

        public SftpOpen() : base(SftpMessageCode.SSH_FXP_OPEN) { }

        public SftpOpen(uint channel, uint requestId, string path, FileModeFlags fileMode, string permissions = "")
            : base(channel, SftpMessageCode.SSH_FXP_OPEN)
        {
            this.RequestId = requestId;
            this.Path = path;
            this.FileMode = fileMode;
            this.Attributes = SftpFileAttributes.ParsePermissions(permissions);
        }
    }

    [Flags]
    public enum FileModeFlags : uint
    {
        SSH_FXF_READ = 0x01,
        SSH_FXF_WRITE = 0x02,
        SSH_FXF_APPEND = 0x04,
        SSH_FXF_CREAT = 0x08,
        SSH_FXF_TRUNC = 0x10,
        SSH_FXF_EXCL = 0x20
    }
}
