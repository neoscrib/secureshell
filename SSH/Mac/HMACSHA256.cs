using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACSHA256 : MacWriter
    {
        public override int KeySize { get { return 32; } }
        public override int BlockSize { get { return 32; } }

        public HMACSHA256() : base() { }

        public HMACSHA256(HashAlgorithm h)
            : base(h)
        { }

        public HMACSHA256(byte[] key)
            : this(new System.Security.Cryptography.HMACSHA256(key))
        { }
    }
}
