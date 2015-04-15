using System;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    class AES128_CBC : CipherCBC
    {
        public override int BlockSize { get { return 16; } }
        public override int KeySize { get { return 16; } }
        internal override Type CryptoType { get { return typeof(RijndaelManaged); } }
    }
}
