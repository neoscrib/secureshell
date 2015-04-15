namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_REQUEST)]
    public class SshUserAuthRequest : Packet
    {
        [SshProperty(1)]
        public string Username { get; set; }
        [SshProperty(2)]
        public string ServiceName { get { return "ssh-connection"; } }
        [SshProperty(3)]
        public string Method { get; set; }

        public SshUserAuthRequest() : base(MessageCode.SSH_MSG_USERAUTH_REQUEST) { }

        public SshUserAuthRequest(string username)
            : this()
        {
            Username = username;
            Method = "none";
        }
    }
}
