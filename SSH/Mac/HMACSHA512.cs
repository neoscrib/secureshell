using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACSHA512 : MacWriter
    {
        public override int KeySize { get { return 64; } }
        public override int BlockSize { get { return 64; } }

        public HMACSHA512() : base() { }

        public HMACSHA512(HashAlgorithm h)
            : base(h)
        { }

        public HMACSHA512(byte[] key)
            : this(new System.Security.Cryptography.HMACSHA512(key))
        { }
    }
}
