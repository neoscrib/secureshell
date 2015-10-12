using System;
using System.Linq;
using System.Numerics;

namespace SSH.MathEC
{
    public struct ECPoint
    {
        public ECCurve Curve { get { return this.curve; } set { this.curve = value; } }
        public BigInteger X { get { return this.x.BigInteger; } set { this.x = new FpBigInteger(value, this.curve.p); } }
        public BigInteger Y { get { return this.y.BigInteger; } set { this.y = new FpBigInteger(value, this.curve.p); } }

        private ECCurve curve;
        private FpBigInteger x;
        private FpBigInteger y;

        public ECPoint(BigInteger x, BigInteger y, ECCurve curve)
            : this()
        {
            this.curve = curve;
            this.x = new FpBigInteger(x, curve.p);
            this.y = new FpBigInteger(y, curve.p);
        }

        public static ECPoint operator +(ECPoint a, ECPoint b)
        {
            if (!a.curve.Equals(b.curve))
                throw new Exception("Points aren't on the same curve.");

            FpBigInteger m;

            if (a.x.Equals(b.x) && (!a.y.Equals(b.y) || a.y.Equals(BigInteger.Zero)))
                throw new Exception("Points are at Infinity.");
            else if (a.Equals(b))
                m = (3 * a.x * a.x + a.curve.a) / (2 * a.y);
            else
                m = (b.y - a.y) / (b.x - a.x);

            var x3 = m * m - a.x - b.x;
            var y3 = m * (a.x - x3) - a.y;
            return new ECPoint(x3.BigInteger, y3.BigInteger, a.curve);
        }

        public static ECPoint operator *(BigInteger d, ECPoint p)
        {
            return p * d;
        }

        public static ECPoint operator *(ECPoint p, BigInteger d)
        {
            // this is actually a left-to-right double and add, but .net's 
            // BigInteger is little-endian and bit strings are naturally big-endian.

            var bytes = d.ToByteArray();
            int x = bytes.Length - 1;
            while (bytes[x] == 0x00) x--;

            var i = 7;
            while ((bytes[x] & (1 << i)) == 0) i--;
            i--;

            var t = p;
            for (; x >= 0; x--)
            {
                for (; i >= 0; i--)
                {
                    t += t;
                    if ((bytes[x] & (1 << i)) != 0)
                        t += p;
                }
                i = 7;
            }
            return t;
        }

        public byte[] ToBlob()
        {
            var length = (int)Math.Ceiling(this.curve.Size / 8d);
            var x1 = this.x.BigInteger.ToByteArray().Reverse().ToArray();
            var y1 = this.y.BigInteger.ToByteArray().Reverse().ToArray();
            var x = new byte[length];
            var y = new byte[length];
            Buffer.BlockCopy(x1, Math.Max(0, x1.Length - length), x, Math.Max(0, length - x1.Length), Math.Min(length, x1.Length));
            Buffer.BlockCopy(y1, Math.Max(0, y1.Length - length), y, Math.Max(0, length - y1.Length), Math.Min(length, y1.Length));

            var blob = new byte[1 + length + length];
            blob[0] = 0x04;
            Buffer.BlockCopy(x, 0, blob, 1, x.Length);
            Buffer.BlockCopy(y, 0, blob, 1 + length, y.Length);

            return blob;
        }

        public static ECPoint FromBlob(byte[] blob)
        {
            var length = (blob.Length - 1) >> 1;
            var x = new byte[length];
            var y = new byte[length];
            Buffer.BlockCopy(blob, 1, x, 0, length);
            Buffer.BlockCopy(blob, 1 + length, y, 0, length);

            var c = length == 32 ? ECCurve.secp256r1 :
                length == 48 ? ECCurve.secp384r1 : ECCurve.secp521r1;
            return new ECPoint(
                BigInteger.Zero.AddUnsignedBigEndian(x), 
                BigInteger.Zero.AddUnsignedBigEndian(y), c);
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", this.x, this.y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ECPoint)) return false;
            var p2 = (ECPoint)obj;
            return this.x.Equals(p2.x) && this.y.Equals(p2.y) && this.curve.Equals(p2.curve);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ECPoint a, object b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ECPoint a, object b)
        {
            return !a.Equals(b);
        }
    }
}
