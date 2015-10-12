using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SSH.IO;
using SSH.MathEC;
using System.IO;

namespace SSH.DiffieHellman
{
    class ECDSAManaged : ECDsa
    {
        int keySize;
        ECCurve curve;
        ECPoint q;
        BigInteger D;

        public ECCurve Curve { get { return curve; } }

        public ECDSAManaged(int keySize)
            : base()
        {
            this.keySize = keySize;
            switch (keySize)
            {
                case 256: curve = ECCurve.secp256r1; break;
                case 384: curve = ECCurve.secp384r1; break;
                case 521: curve = ECCurve.secp521r1; break;
                default: throw new CryptographicException("ECDiffieHellmanManaged only supports 256, 384 and 521 bit key sizes.");
            }
        }

        public override int KeySize { get { return keySize; } set { keySize = value; } }

        public override string SignatureAlgorithm
        {
            get
            {
                return string.Format("ecdsa-sha2-nistp{0}", KeySize);
            }
        }

        public void ImportParameters(ECDiffieHellmanPublicKey otherPartyPublicKey)
        {
            var opp = otherPartyPublicKey.ToByteArray();
            q = ECPoint.FromBlob(opp);
        }

        public void ImportParameters(ECDiffieHellmanManagedPrivateKey privateKey)
        {
            D = privateKey.D;
            q = ECPoint.FromBlob(privateKey.PublicKey.ToByteArray());
        }

        public override bool VerifyHash(byte[] rgbHash, byte[] rgbSignature)
        {
            BigInteger r, s;
            var n = curve.n;
            var e = CalculateE(rgbHash);
            var g = curve.G;

            using (var pr = new PacketReader(rgbSignature))
            {
                r = BigInteger.Zero.AddUnsignedBigEndian(pr.ReadString());
                s = BigInteger.Zero.AddUnsignedBigEndian(pr.ReadString());
            }

            var val = s.ModInverse(n);
            var u1 = (e * val) % n;
            var u2 = (r * val) % n;
            var R = (u1 * g) + (u2 * q);
            var v = R.X % n;
            return v == r;
        }

        private BigInteger CalculateE(byte[] hash)
        {
            int num = hash.Length * 8;
            BigInteger e = BigInteger.Zero.AddUnsignedBigEndian(hash);
            if (curve.Size < num)
                e = BigInteger.Zero.AddUnsignedBigEndian(hash.Take(curve.Size / 8).ToArray());
            return e;
        }

        public override byte[] SignHash(byte[] hash)
        {
            var ephemeral = new ECDiffieHellmanManagedPrivateKey(curve);
            var n = curve.n;
            var k = ephemeral.D;
            var R = ECPoint.FromBlob(ephemeral.PublicKey.ToByteArray());
            var r = R.X % n;

            var e = CalculateE(hash);
            var d = D;
            var val = k.ModInverse(n);
            var s = (val * (e + (r * d))) % n;

            using (var pw = new PacketWriter())
            {
                pw.WriteString(r.ToByteArray().Reverse().ToArray());
                pw.WriteString(s.ToByteArray().Reverse().ToArray());
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }

        public override void FromXmlString(string xmlString)
        {
            throw new NotImplementedException();
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            throw new NotImplementedException();
        }
    }
}
