using System.Diagnostics;
using SSH.DiffieHellman;
using SSH.IO;
using SSH.Packets;
using SSH.Curve25519;
using System.Security.Cryptography;

namespace SSH.Processor
{
    class ECDHProcessor : DiffieHellmanProcessor
    {
        private ECDiffieHellman ecdh;
        private byte[] ServerPublicKey;

        public ECDHProcessor(Session session)
            : base(session)
        { }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_KEX_ECDH_REPLY:
                    var msg = (SshECDHKexReply)p;
                    ServerHostKey = msg.K;
                    ServerSignature = msg.S;
                    ServerPublicKey = msg.Q;
                    SharedSecret = ecdh.DeriveKeyMaterial(new ECDiffieHellmanManagedPublicKey(ServerPublicKey));
                    ExchangeHash = CalculateExchangeHash();
                    Close(Verify());
                    return true;
            }

            return false;
        }

        public override void Initialize()
        {
            var kexAlgorithm = session.Algorithms.KexAlgorithms[0];

            switch (kexAlgorithm)
            {
                case "curve25519-sha256@libssh.org": ecdh = new Curve25519Managed(); break;
                case "ecdh-sha2-nistp521": ecdh = new ECDiffieHellmanManaged(MathEC.ECCurve.secp521r1); break;
                case "ecdh-sha2-nistp384": ecdh = new ECDiffieHellmanManaged(MathEC.ECCurve.secp384r1); break;
                case "ecdh-sha2-nistp256":
                default: ecdh = new ECDiffieHellmanManaged(MathEC.ECCurve.secp256r1); break;
            }

            var ecdhInit = new SshECDHKexInit(ecdh.PublicKey.ToByteArray());
            session.Socket.WritePacket(ecdhInit);
        }

        private byte[] CalculateExchangeHash()
        {
            using (var pw = HashWriter.Create(session.Algorithms.KexAlgorithms[0]))
            {
                pw.WriteString(session.ClientIdentifier.ToSshMessage());
                pw.WriteString(session.ServerIdentifier.ToSshMessage());
                pw.WriteString(session.KexProcessor.ClientKexInit.ToSshMessage());
                pw.WriteString(session.KexProcessor.ServerKexInit.ToSshMessage());
                pw.WriteString(ServerHostKey);
                pw.WriteString(ecdh.PublicKey.ToByteArray());
                pw.WriteString(ServerPublicKey);
                pw.WriteString(SharedSecret);
                return pw.Hash;
            }
        }
    }
}
