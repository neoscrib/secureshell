using System;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    class TripleDES_CTR : CipherCTR
    {
        public override int BlockSize { get { return 8; } }
        public override int KeySize { get { return 24; } }
        internal override Type CryptoType { get { return typeof(TripleDESCryptoServiceProvider); } }
    }
}
