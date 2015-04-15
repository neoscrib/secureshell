namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_FAILURE)]
    public class SshUserAuthFailure : Packet
    {
        [SshProperty(1)]
        public string Methods { get; set; }
        [SshProperty(2)]
        public bool PartialSuccess { get; set; }

        public SshUserAuthFailure() : base(MessageCode.SSH_MSG_USERAUTH_FAILURE) { }
    }
}
