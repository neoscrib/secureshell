using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Identity
{
    [IdentityFile("PuTTY-User-Key-File-2: ssh-rsa")]
    public class PuttyRSAIdentity : PuttyIdentity
    {
        private RSAParameters parameters;

        public PuttyRSAIdentity(string path)
            : base(path)
        {
            Algorithm = new RSACryptoServiceProvider();
        }

        public override byte[] PublicKey
        {
            get
            {
                Decrypt();
                var pw = new PacketWriter();
                pw.WriteBytes(AlgorithmName);
                pw.WriteString(parameters.Exponent);
                pw.WriteString(parameters.Modulus);
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }

        public override void ExtractParameters()
        {
            if (parameters.Equals(default(RSAParameters)))
                if ((blob[0] << 24 | blob[1] << 16 | blob[2] << 8 | blob[3]) == 7)
                    parameters = ExtractParameters(blob);
        }

        private RSAParameters ExtractParameters(byte[] blob)
        {
            using (var pr = new PacketReader(blob))
            {
                pr.ReadStringAsString(); // ssh-rsa
                var key = new RSAParameters();
                key.Exponent = pr.ReadString();
                key.Modulus = pr.ReadString();
                key.D = pr.ReadString();
                key.P = pr.ReadString();
                key.Q = pr.ReadString();
                key.InverseQ = pr.ReadString();
                var d = new BigInteger(key.D.Reverse().ToArray());
                var p = new BigInteger(key.P.Reverse().ToArray());
                var q = new BigInteger(key.Q.Reverse().ToArray());
                key.DP = (d % (p - 1)).ToByteArray().Reverse().ToArray();
                key.DQ = (d % (q - 1)).ToByteArray().Reverse().ToArray();
                return key;
            }
        }

        public override byte[] Sign(byte[] data)
        {
            var key = parameters;
            key.Modulus = key.Modulus.TrimLeft(0);
            key.Exponent = key.Exponent.TrimLeft(0);
            key.D = key.D.TrimLeft(0);
            key.P = key.P.TrimLeft(0);
            key.Q = key.Q.TrimLeft(0);
            key.DP = key.DP.TrimLeft(0);
            key.DQ = key.DQ.TrimLeft(0);
            key.InverseQ = key.InverseQ.TrimLeft(0);

            var rsa = (RSACryptoServiceProvider)Algorithm;
            rsa.ImportParameters(key);
            return rsa.SignData(data, new SHA1CryptoServiceProvider());
        }
    }
}
