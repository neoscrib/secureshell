using SSH.DiffieHellman;
using SSH.MathEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SSH.Curve25519
{
    class Curve25519Managed : ECDiffieHellman
    {
        private Curve25519ManagedPrivateKey privateKey;

        public override ECDiffieHellmanPublicKey PublicKey
        {
            get
            {
                return privateKey.PublicKey;
            }
        }

        public Curve25519Managed()
        {
            privateKey = new Curve25519ManagedPrivateKey();
        }

        public Curve25519Managed(Curve25519ManagedPrivateKey key)
        {
            privateKey = key;
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

        public override void FromXmlString(string xmlString)
        {
            throw new NotImplementedException();
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            throw new NotImplementedException();
        }
    }

    public class Curve25519ManagedPrivateKey
    {
        public ECDiffieHellmanManagedPublicKey PublicKey { get; private set; }
        internal BigInteger D { get; private set; }
        private ECCurve curve = new ECCurve()
        {
            Size = 256,
            p = BigInteger.Pow(2, 255) - 19,
            a = 121666
        };

        public Curve25519ManagedPrivateKey()
        {
            var d = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(d);
            d[0] &= 248;
            d[31] &= 127;
            d[31] |= 64;
            this.D = BigInteger.Zero.AddUnsignedBigEndian(d);
            this.PublicKey = new ECDiffieHellmanManagedPublicKey(new ECPoint(9, 1, curve) * this.D);
        }

        public Curve25519ManagedPrivateKey(byte[] privateKey)
        {
            this.D = BigInteger.Zero.AddUnsignedBigEndian(privateKey);
            this.PublicKey = new ECDiffieHellmanManagedPublicKey(new ECPoint(9, 1, curve) * this.D);
        }

        public Curve25519ManagedPrivateKey(BigInteger d)
        {
            this.D = d;
            this.PublicKey = new ECDiffieHellmanManagedPublicKey(new ECPoint(9, 1, curve) * this.D);
        }
    }
}
