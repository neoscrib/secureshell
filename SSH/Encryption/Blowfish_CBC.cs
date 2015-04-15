using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Encryption
{
    class Blowfish_CBC : CipherCBC
    {
        public override int BlockSize { get { return 8; } }
        public override int KeySize { get { return 16; } }
        internal override Type CryptoType { get { return typeof(BlowfishManaged); } }
    }
}
