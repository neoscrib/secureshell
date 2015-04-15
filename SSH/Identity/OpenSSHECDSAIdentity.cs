using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using SSH.MathEC;
using SSH.DiffieHellman;
using SSH.IO;
using System.IO;
using System.Security.Cryptography;

namespace SSH.Identity
{
    [IdentityFile("-----BEGIN EC PRIVATE KEY-----")]
    public class OpenSSHECDSAIdentity : OpenSSHIdentity
    {
        private ECDiffieHellmanManagedPrivateKey privateKey;
        private int keySize = 256;

        public OpenSSHECDSAIdentity(string path)
            : base(path)
        { }

        public override byte[] PublicKey
        {
            get
            {
                Decrypt();
                var pw = new PacketWriter();
                pw.WriteBytes(AlgorithmName);
                pw.WriteBytes(AlgorithmName.Replace("ecdsa-sha2-", ""));
                pw.WriteString(privateKey.PublicKey.ToByteArray());
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }

        public override void ExtractParameters()
        {
            if (privateKey == null)
            {
                var parameters = ExtractParameters(blob);
                var d = BigInteger.Zero.AddUnsignedBigEndian(parameters);

                keySize = (int)BigInteger.Log(d, 2);
                if (keySize > 160 && keySize < 256) keySize = 256;
                if (keySize > 256 && keySize < 384) keySize = 384;
                if (keySize > 384 && keySize < 521) keySize = 521;

                var ecdsa = new ECDSAManaged(keySize);
                var curve = ecdsa.Curve;
                privateKey = new ECDiffieHellmanManagedPrivateKey(curve, d);
                ecdsa.ImportParameters(privateKey);
                Algorithm = ecdsa;
            }
        }

        private byte[] ExtractParameters(byte[] blob)
        {
            byte[] parameters;
            int i = 0;
            if (blob[i++] == 0x30)
            {
                var length = blob[i++];
                if ((length & 0x80) != 0)
                    length = blob[i++];
                if (blob[i++] == 0x02 && blob[i++] == 0x01 && blob[i++] == 0x01 && blob[i++] == 0x04)
                {
                    length = blob[i++];
                    parameters = new byte[length];
                    Buffer.BlockCopy(blob, i, parameters, 0, length);
                    return parameters;
                }
            }
            return null;
        }

        public override byte[] Sign(byte[] data)
        {
            Decrypt();
            byte[] H;
            using (var hw = HashWriter.Create(AlgorithmName))
            {
                hw.Write(data);
                H = hw.Hash;
            }

            return ((ECDSAManaged)Algorithm).SignHash(H);
        }
    }
}
