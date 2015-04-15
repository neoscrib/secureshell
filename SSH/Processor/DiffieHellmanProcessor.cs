using System.Diagnostics;
using System.Security.Cryptography;
using SSH.DiffieHellman;
using SSH.IO;
using SSH.Packets;

namespace SSH.Processor
{
    abstract class DiffieHellmanProcessor : PacketProcessor, IKeyExchangeProcessor
    {
        internal byte[] ServerHostKey { get; set; }
        internal byte[] ServerSignature { get; set; }
        public byte[] SharedSecret { get; set; }
        public byte[] ExchangeHash { get; set; }

        public DiffieHellmanProcessor(Session session) : base(session) { }

        public abstract override bool InternalProcessPacket(IPacket p);

        public abstract void Initialize();

        public new void Close(StatusCode code)
        {
            base.Close(code);
            switch (code)
            {
                case SSH.StatusCode.SignatureVerificationFailed:
                case SSH.StatusCode.HostVerificationFailed:
                    session.Disconnect(SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_HOST_KEY_NOT_VERIFIABLE);
                    break;
                default:
                    break;
            }
        }

        public StatusCode Verify()
        {
            var code = session.VerifyHostKey(ServerHostKey);
            if (code != SSH.StatusCode.OK)
                return code;

            byte[] sig = ExtractSignature();
            byte[] h = HashExchangeHash();
            bool result = VerifySignature(sig, h);

            if (result)
            {
                Debug.WriteLine("Signature verified!");
                return StatusCode.OK;
            }
            else
            {
                Debug.WriteLine("The signature could not be verified!");
                return StatusCode.SignatureVerificationFailed;
            }
        }

        private bool VerifySignature(byte[] sig, byte[] h)
        {
            string algorithm = string.Empty;
            using (var pr = new PacketReader(ServerHostKey))
            {
                algorithm = pr.ReadStringAsString();
                if (algorithm == "ssh-rsa")
                    return VerifyRSA(sig, h, pr);
                else if (algorithm == "ssh-dss")
                    return VerifyDSA(sig, h, pr);
                else if (algorithm.StartsWith("ecdsa-sha2-"))
                    return VerifyECDSA(sig, h, pr);
            }
            return false;
        }

        private static bool VerifyECDSA(byte[] sig, byte[] h, PacketReader pr)
        {
            var keyAlgorithm = pr.ReadStringAsString();
            var keySize = int.Parse(keyAlgorithm.Substring(keyAlgorithm.Length - 3));
            var keyInfo = pr.ReadString();

            var ecdsa = new ECDSAManaged(keySize);
            ecdsa.ImportParameters(new ECDiffieHellmanManagedPublicKey(keyInfo));
            return ecdsa.VerifyHash(h, sig);
        }

        private static bool VerifyDSA(byte[] sig, byte[] h, PacketReader pr)
        {
            var keyInfo = new DSAParameters();
            keyInfo.P = pr.ReadString().TrimLeft(0);
            keyInfo.Q = pr.ReadString().TrimLeft(0);
            keyInfo.G = pr.ReadString().TrimLeft(0);
            keyInfo.Y = pr.ReadString().TrimLeft(0);

            var dsa = new DSACryptoServiceProvider();
            dsa.ImportParameters(keyInfo);
            return dsa.VerifyHash(h, "SHA1", sig);
        }

        private static bool VerifyRSA(byte[] sig, byte[] h, PacketReader pr)
        {
            var keyInfo = new RSAParameters();
            keyInfo.Exponent = pr.ReadString();
            keyInfo.Modulus = pr.ReadString().TrimLeft(0);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(keyInfo);
            return rsa.VerifyHash(h, "SHA1", sig);
        }

        private byte[] HashExchangeHash()
        {
            var h = new byte[0];
            using (var pw = HashWriter.Create(session.Algorithms.KeyAlgorithmsServer[0]))
            {
                pw.Write(ExchangeHash, 0, ExchangeHash.Length);
                h = pw.Hash;
            }
            return h;
        }

        private byte[] ExtractSignature()
        {
            byte[] sig;
            using (var pr1 = new PacketReader(ServerSignature))
            {
                pr1.ReadString();
                sig = pr1.ReadString();
            }
            return sig;
        }
    }
}
