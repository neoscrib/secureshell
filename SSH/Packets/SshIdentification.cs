namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_IDENTIFICATION)]
    public class SshIdentification : Packet
    {
        [SshProperty(1)]
        public string Identifier { get; set; }

        public SshIdentification() : this("SSH-2.0-Gate10_1.1") { }

        public SshIdentification(byte[] data) : this(System.Text.Encoding.Default.GetString(data)) { }

        public SshIdentification(string identifier)
            : base(MessageCode.SSH_MSG_IDENTIFICATION)
        {
            this.Identifier = identifier;
        }
    }
}
