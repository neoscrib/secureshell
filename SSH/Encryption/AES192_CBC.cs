using System;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    class AES192_CBC : CipherCBC
    {
        public override int BlockSize { get { return 16; } }
        public override int KeySize { get { return 24; } }
        internal override Type CryptoType { get { return typeof(RijndaelManaged); } }
    }
}
