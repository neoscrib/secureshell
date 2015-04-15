using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSH.IO;
using System.Security.Cryptography;

namespace SSH.Mac
{
    class HMACRIPEMD160 : MacWriter
    {
        public override int KeySize { get { return 20; } }
        public override int BlockSize { get { return 20; } }

        public HMACRIPEMD160() : base() { }

        public HMACRIPEMD160(HashAlgorithm h)
            : base(h)
        { }

        public HMACRIPEMD160(byte[] key)
            : this(new System.Security.Cryptography.HMACRIPEMD160(key))
        { }
    }
}
