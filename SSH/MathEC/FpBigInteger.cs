using System;
using System.Linq;
using System.Numerics;

namespace SSH.MathEC
{
    public struct FpBigInteger
    {
        BigInteger bi;
        BigInteger p;

        public BigInteger BigInteger { get { return bi; } set { bi = value; } }
        public BigInteger Modulus { get { return p; } set { p = value; } }

        public FpBigInteger(byte[] bytes)
        {
            var bytes2 = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, bytes2, 1, bytes.Length);
            this.bi = new BigInteger(bytes2.Reverse().ToArray());
            this.p = bi;
        }

        public FpBigInteger(BigInteger i)
            : this(i, i)
        { }

        public FpBigInteger(BigInteger i, BigInteger p)
        {
            this.bi = i;
            this.p = p;
        }

        public FpBigInteger(BigInteger i, FpBigInteger p)
            : this(i, p.BigInteger)
        { }

        public static FpBigInteger operator +(BigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a + b.BigInteger, b.Modulus) % b.Modulus;
        }

        public static FpBigInteger operator +(FpBigInteger a, BigInteger b)
        {
            return new FpBigInteger(a.BigInteger + b, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator +(FpBigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a.BigInteger + b.BigInteger, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator -(BigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a - b.BigInteger, b.Modulus) % b.Modulus;
        }

        public static FpBigInteger operator -(FpBigInteger a, BigInteger b)
        {
            return new FpBigInteger(a.BigInteger - b, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator -(FpBigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a.BigInteger - b.BigInteger, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator /(BigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a * b.BigInteger.ModInverse(b.Modulus), b.Modulus) % b.Modulus;
        }

        public static FpBigInteger operator /(FpBigInteger a, BigInteger b)
        {
            return new FpBigInteger(a.BigInteger * b.ModInverse(a.Modulus), a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator /(FpBigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a.BigInteger * b.BigInteger.ModInverse(a.Modulus), a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator *(BigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a * b.BigInteger, b.Modulus) % b.Modulus;
        }

        public static FpBigInteger operator *(FpBigInteger a, BigInteger b)
        {
            return new FpBigInteger(a.BigInteger * b, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator *(FpBigInteger a, FpBigInteger b)
        {
            return new FpBigInteger(a.BigInteger * b.BigInteger, a.Modulus) % a.Modulus;
        }

        public static FpBigInteger operator %(FpBigInteger a, BigInteger m)
        {
            if (m.Sign < 1)
                throw new ArithmeticException("Modulus must be positive");
            BigInteger bi = a.BigInteger % m;
            if (bi.Sign < 0)
                return new FpBigInteger(bi + m, a.Modulus);
            else
                return new FpBigInteger(bi, a.Modulus);
        }

        public static bool operator <(BigInteger a, FpBigInteger b)
        {
            return a < b.BigInteger;
        }

        public static bool operator <(FpBigInteger a, BigInteger b)
        {
            return a.BigInteger < b;
        }

        public static bool operator >(BigInteger a, FpBigInteger b)
        {
            return a > b.BigInteger;
        }

        public static bool operator >(FpBigInteger a, BigInteger b)
        {
            return a.BigInteger > b;
        }

        public static bool operator ==(FpBigInteger a, FpBigInteger b)
        {
            return a.BigInteger == b.BigInteger;
        }

        public static bool operator ==(FpBigInteger a, BigInteger b)
        {
            return a.BigInteger == b;
        }

        public static bool operator !=(FpBigInteger a, FpBigInteger b)
        {
            return a.BigInteger != b.BigInteger;
        }

        public static bool operator !=(FpBigInteger a, BigInteger b)
        {
            return a.BigInteger != b;
        }

        public override string ToString()
        {
            return this.BigInteger.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is FpBigInteger)
                return this.BigInteger.Equals(((FpBigInteger)obj).BigInteger);
            else if (obj is BigInteger)
                return this.BigInteger.Equals(obj);
            return false;
        }

        public override int GetHashCode()
        {
            return this.BigInteger.GetHashCode();
        }
    }

    static class Extensions
    {
        public static BigInteger ModInverse(this BigInteger b, BigInteger mod)
        {
            BigInteger x = 1;
            BigInteger y = 0;
            BigInteger a = mod;
            BigInteger q, t;
            while (b != 0)
            {
                t = b;
                q = a / t;
                b = a - q * t;
                a = t;
                t = x;
                x = y - q * t;
                y = t;
            }

            return y + (y < 0 ? mod : 0);
        }

        public static BigInteger AddUnsignedBigEndian(this BigInteger b, byte[] bytes)
        {
            var bytes2 = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, bytes2, 1, bytes.Length);
            return b + new BigInteger(bytes2.Reverse().ToArray());
        }
    }
}
