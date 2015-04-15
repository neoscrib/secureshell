namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_DEBUG)]
    public class SshDebug : Packet
    {
        [SshProperty(1)]
        public bool Display { get; set; }
        [SshProperty(2)]
        public string Message { get; set; }
        [SshProperty(3)]
        public string Language { get; set; }

        public SshDebug() : base(MessageCode.SSH_MSG_DEBUG) { }
    }
}
