using SSH.Identity;

namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_REQUEST)]
    public class SshUserAuthRequestPublicKey : SshUserAuthRequest
    {
        private bool sigIncluded = false;

        [SshProperty(4)]
        public bool SignatureIncluded { get { return sigIncluded; } private set { sigIncluded = value; } }
        [SshProperty(5)]
        public string AlgorithmName { get; set; }
        [SshProperty(6)]
        public byte[] PublicKey { get; set; }

        public SshUserAuthRequestPublicKey() : base() { }

        public SshUserAuthRequestPublicKey(string username, IIdentityFile identity)
            : this()
        {
            Username = username;
            Method = "publickey";
            AlgorithmName = identity.AlgorithmName;
            PublicKey = identity.PublicKey;
        }

        internal static SshUserAuthRequestPublicKey CreateForSignature(string username, IIdentityFile identity)
        {
            var p = new SshUserAuthRequestPublicKey(username, identity) { SignatureIncluded = true };
            return p;
        }
    }
}
