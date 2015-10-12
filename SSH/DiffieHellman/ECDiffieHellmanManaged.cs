using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SSH.MathEC;

namespace SSH.DiffieHellman
{
    class ECDiffieHellmanManaged : ECDiffieHellman
    {
        private ECDiffieHellmanManagedPrivateKey privateKey;

        public ECDiffieHellmanManaged()
            : this(256)
        { }

        public ECDiffieHellmanManaged(ECDiffieHellmanManagedPrivateKey key)
        {
            privateKey = key;
        }

        public ECDiffieHellmanManaged(ECCurve curve)
        {
            privateKey = new ECDiffieHellmanManagedPrivateKey(curve);
        }

        public ECDiffieHellmanManaged(int keySize)
        {
            var curve = ECCurve.secp256r1;
            switch (keySize)
            {
                case 256: curve = ECCurve.secp256r1; break;
                case 384: curve = ECCurve.secp384r1; break;
                case 521: curve = ECCurve.secp521r1; break;
                default: throw new CryptographicException("ECDiffieHellmanManaged only supports 256, 384 and 521 bit key sizes.");
            }
            privateKey = new ECDiffieHellmanManagedPrivateKey(curve);
        }

        public override byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey)
        {
            var d = privateKey.D;

            // read the other side's public key.
            var opp = otherPartyPublicKey.ToByteArray();
            ECPoint Qs = ECPoint.FromBlob(opp);

            // multiply their public key with our private d to get the shared secret.
            var p = Qs * d;
            var z = p.X.ToByteArray().Reverse().ToArray();
            return z;
        }

        public override ECDiffieHellmanPublicKey PublicKey
        {
            get { return privateKey.PublicKey; }
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

    class ECDiffieHellmanManagedPrivateKey
    {
        ECCurve curve;

        public ECDiffieHellmanManagedPublicKey PublicKey { get; private set; }
        internal BigInteger D { get; private set; }

        public ECCurve Curve
        {
            get { return curve; }
        }

        public ECDiffieHellmanManagedPrivateKey(ECCurve curve)
        {
            this.curve = curve;
            var n1 = curve.n.ToByteArray().Reverse().ToArray().TrimLeft(0x00);
            var d1 = new byte[n1.Length];
            var rng = new RNGCryptoServiceProvider();
            while (D < 1 || D >= curve.n)
            {
                rng.GetBytes(d1);
                d1[0] %= n1[0];
                D = BigInteger.Zero.AddUnsignedBigEndian(d1);
            }
            PublicKey = new ECDiffieHellmanManagedPublicKey(curve.G * D);
        }

        public ECDiffieHellmanManagedPrivateKey(ECCurve curve, BigInteger d)
        {
            this.curve = curve;
            this.D = d;
            this.PublicKey = new ECDiffieHellmanManagedPublicKey(curve.G * D);
        }
    }

    public class ECDiffieHellmanManagedPublicKey : ECDiffieHellmanPublicKey
    {
        public ECDiffieHellmanManagedPublicKey(byte[] keyBlob)
            : base(keyBlob)
        { }

        internal ECDiffieHellmanManagedPublicKey(ECPoint point)
            : this(point.ToBlob())
        { }

        public override string ToXmlString()
        {
            throw new NotImplementedException();
        }
    }
}
