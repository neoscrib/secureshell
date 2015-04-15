using System;
using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Mac
{
    class HMACSHA196 : HMACSHA1
    {
        public override int KeySize { get { return 20; } }
        public override int BlockSize { get { return 12; } }

        public HMACSHA196() : base() { }

        public HMACSHA196(HashAlgorithm h)
            : base(h)
        { }

        public HMACSHA196(byte[] key)
            : this(new System.Security.Cryptography.HMACSHA1(key))
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
