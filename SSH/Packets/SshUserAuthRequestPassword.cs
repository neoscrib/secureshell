namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_REQUEST)]
    public class SshUserAuthRequestPassword : SshUserAuthRequest
    {
        [SshProperty(4)]
        public bool ChangePassword { get { return false; } }
        [SshProperty(5)]
        public string Password { get; set; }

        public SshUserAuthRequestPassword() : base() { }

        public SshUserAuthRequestPassword(string username, string password)
            : this()
        {
            Username = username;
            Method = "password";
            Password = password;
        }
    }
}
