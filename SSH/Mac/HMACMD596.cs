using System;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACMD596 : HMACMD5
    {
        public override int KeySize { get { return 16; } }
        public override int BlockSize { get { return 12; } }

        public HMACMD596() : base() { }

        public HMACMD596(HashAlgorithm h)
            : base(h)
        { }

        public HMACMD596(byte[] key)
            : this(new System.Security.Cryptography.HMACMD5(key))
        { }

        public override byte[] Hash
        {
            get
            {
                byte[] hash = new byte[BlockSize];
                Buffer.BlockCopy(base.Hash, 0, hash, 0, BlockSize);
                return hash;
            }
        }
    }
}
