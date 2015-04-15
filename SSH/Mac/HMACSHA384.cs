using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSH.IO;
using System.Security.Cryptography;

namespace SSH.Mac
{
    class HMACSHA384 : MacWriter
    {
        public override int KeySize { get { return 48; } }
        public override int BlockSize { get { return 48; } }

        public HMACSHA384() : base() { }

        public HMACSHA384(HashAlgorithm h)
            : base(h)
        { }

        public HMACSHA384(byte[] key)
            : this(new System.Security.Cryptography.HMACSHA384(key))
        { }
    }
}
