using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACMD5 : MacWriter
    {
        public override int KeySize { get { return 16; } }
        public override int BlockSize { get { return 16; } }

        public HMACMD5() : base() { }

        public HMACMD5(HashAlgorithm h)
            : base(h)
        { }

        public HMACMD5(byte[] key)
            : this(new System.Security.Cryptography.HMACMD5(key))
        { }
    }
}
