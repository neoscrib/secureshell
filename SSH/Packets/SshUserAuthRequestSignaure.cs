using System.IO;
using SSH.Identity;
using SSH.IO;

namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_REQUEST)]
    public class SshUserAuthRequestSignature : SshUserAuthRequestPublicKey
    {
        [SshProperty(4)]
        public new bool SignatureIncluded { get { return true; } private set { } }
        [SshProperty(7)]
        public byte[] Signature { get; set; }

        public SshUserAuthRequestSignature() : base() { }

        public SshUserAuthRequestSignature(string username, IIdentityFile identity, byte[] sessionId)
            : base(username, identity)
        {
            var buffer = SshUserAuthRequestPublicKey.CreateForSignature(username, identity).ToSshMessage();
            using (var pw1 = new PacketWriter())
            {
                pw1.WriteString(sessionId);
                pw1.Write(buffer);
                using (var pw2 = new PacketWriter())
                {
                    pw2.WriteBytes(identity.AlgorithmName);
                    pw2.WriteString(identity.Sign(((MemoryStream)pw1.BaseStream).ToArray()));
                    Signature = ((MemoryStream)pw2.BaseStream).ToArray();
                }
            }
        }
    }
}
