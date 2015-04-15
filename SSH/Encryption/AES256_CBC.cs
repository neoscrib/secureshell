using System;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    class AES256_CBC : CipherCBC
    {
        public override int BlockSize { get { return 16; } }
        public override int KeySize { get { return 32; } }
        internal override Type CryptoType { get { return typeof(RijndaelManaged); } }
    }
}
