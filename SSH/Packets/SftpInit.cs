using System.Collections.Generic;

namespace SSH.Packets
{
    [SftpPacket(SftpMessageCode.SSH_FXP_INIT)]
    [SftpPacket(SftpMessageCode.SSH_FXP_VERSION)]
    class SftpInit : SftpPacket
    {
        [SshProperty(3)]
        public uint Version { get; set; }
        [SshProperty(4)]
        public Dictionary<string, string> ExtensionData { get; set; }

        public SftpInit() : base(SftpMessageCode.SSH_FXP_INIT) { }

        public SftpInit(uint channel, uint version)
            : base(channel, SftpMessageCode.SSH_FXP_INIT)
        {
            this.Version = version;
            this.ExtensionData = new Dictionary<string, string>();
        }
    }
}
