namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_PK_OK)]
    public class SshUserAuthPublicKeyOk : Packet
    {
        [SshProperty(1)]
        public string AlgorithmName { get; set; }
        [SshProperty(2)]
        public byte[] PublicKeyBlob { get; set; }

        public SshUserAuthPublicKeyOk() : base(MessageCode.SSH_MSG_USERAUTH_PK_OK) { }
    }
}
