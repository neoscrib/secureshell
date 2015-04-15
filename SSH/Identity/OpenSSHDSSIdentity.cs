using System;
using System.IO;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Identity
{
    [IdentityFile("-----BEGIN DSA PRIVATE KEY-----")]
    public class OpenSSHDSSIdentity : OpenSSHIdentity
    {
        private DSAParameters parameters;

        public override byte[] PublicKey
        {
            get
            {
                Decrypt();
                var pw = new PacketWriter();
                pw.WriteBytes(AlgorithmName);
                pw.WriteString(parameters.P);
                pw.WriteString(parameters.Q);
                pw.WriteString(parameters.G);
                pw.WriteString(parameters.Y);
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }

        public OpenSSHDSSIdentity(string path)
            : base(path)
        {
            Algorithm = new DSACryptoServiceProvider();
        }

        public override void ExtractParameters()
        {
            if (parameters.Equals(default(DSAParameters)))
            {
                if (blob[0] == 0x30 && blob[1] == 0x82)
                {
                    parameters = ExtractParameters(blob);
                }
            }
        }

        private DSAParameters ExtractParameters(byte[] blob)
        {
            int length = blob[2] << 8 | blob[3];
            byte[] sequence = new byte[length];
            Buffer.BlockCopy(blob, 4, sequence, 0, length);

            byte[][] data = new byte[5][];
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

            var key = new DSAParameters();
            key.P = data[0];
            key.Q = data[1];
            key.G = data[2];
            key.Y = data[3];
            key.X = data[4];
            return key;
        }

        public override byte[] Sign(byte[] data)
        {
            var key = parameters;
            key.P = key.P.TrimLeft(0);
            key.Q = key.Q.TrimLeft(0);
            key.G = key.G.TrimLeft(0);
            key.Y = key.Y.TrimLeft(0);
            key.X = key.X.TrimLeft(0);

            var dsa = (DSACryptoServiceProvider)Algorithm;
            dsa.ImportParameters(key);
            return dsa.SignData(data);
        }
    }
}
