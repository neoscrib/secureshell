using System;

namespace SSH.Encryption
{
    class Cast128_CBC : CipherCBC
    {
        public override int BlockSize { get { return 8; } }
        public override int KeySize { get { return 16; } }
        internal override Type CryptoType { get { return typeof(CastManaged); } }
    }
}
