using System.IO;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Identity
{
    [IdentityFile("PuTTY-User-Key-File-2: ssh-dss")]
    public class PuttyDSSIdentity : PuttyIdentity
    {
        private DSAParameters parameters;

        public PuttyDSSIdentity(string path)
            : base(path)
        {
            Algorithm = new DSACryptoServiceProvider();
        }

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

        public override void ExtractParameters()
        {
            if (parameters.Equals(default(DSAParameters)))
                if ((blob[0] << 24 | blob[1] << 16 | blob[2] << 8 | blob[3]) == 7)
                    parameters = ExtractParameters(blob);
        }

        private DSAParameters ExtractParameters(byte[] blob)
        {
            using (var pr = new PacketReader(blob))
            {
                pr.ReadStringAsString(); // ssh-dss
                var key = new DSAParameters();
                key.P = pr.ReadString();
                key.Q = pr.ReadString();
                key.G = pr.ReadString();
                key.Y = pr.ReadString();
                key.X = pr.ReadString();
                return key;
            }
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
