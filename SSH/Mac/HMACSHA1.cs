using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACSHA1 : MacWriter
    {
        public override int KeySize { get { return 20; } }
        public override int BlockSize { get { return 20; } }

        public HMACSHA1() : base() { }

        public HMACSHA1(HashAlgorithm h)
            : base(h)
        { }

        public HMACSHA1(byte[] key)
            : this(new System.Security.Cryptography.HMACSHA1(key))
        { }
    }
}
