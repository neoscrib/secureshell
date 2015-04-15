using System;
using System.IO;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Identity
{
    [IdentityFile("-----BEGIN RSA PRIVATE KEY-----")]
    public class OpenSSHRSAIdentity : OpenSSHIdentity
    {
        private RSAParameters parameters;

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

        public OpenSSHRSAIdentity(string path)
            : base(path)
        {
            Algorithm = new RSACryptoServiceProvider();
        }

        public override void ExtractParameters()
        {
            if (parameters.Equals(default(RSAParameters)))
            {
                if (blob[0] == 0x30 && blob[1] == 0x82)
                {
                    parameters = ExtractParameters(blob);
                }
            }
        }

        private RSAParameters ExtractParameters(byte[] blob)
        {
            int length = blob[2] << 8 | blob[3];
            byte[] sequence = new byte[length];
            Buffer.BlockCopy(blob, 4, sequence, 0, length);

            byte[][] data = new byte[8][];
            for (int i = 0, index = -1; i < sequence.Length; i += length, index++)
            {
                i++;
                length = sequence[i++];
                if ((length & 0x80) != 0)
                {
                    var length2 = length & 0x7F;
                    length = 0;
                    while (length2-- > 0) length = length << 8 | sequence[i++];
                }
                else
                {
                    length = length & 0x7F;
                }

                if (index > -1)
                {
                    data[index] = new byte[length];
                    Buffer.BlockCopy(sequence, i, data[index], 0, length);
                }
            }

            var key = new RSAParameters();
            key.Modulus = data[0];
            key.Exponent = data[1];
            key.D = data[2];
            key.P = data[3];
            key.Q = data[4];
            key.DP = data[5];
            key.DQ = data[6];
            key.InverseQ = data[7];
            return key;
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
