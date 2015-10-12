using System;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    abstract class CipherECB : Cipher
    {
        public unsafe override void Initialize(CipherMode mode, byte[] key, byte[] iv, PaddingMode padding = PaddingMode.None)
        {
            Mode = mode;
            Crypto = Activator.CreateInstance(CryptoType) as SymmetricAlgorithm;
            Crypto.BlockSize = BlockSize * 8;
            Crypto.Mode = System.Security.Cryptography.CipherMode.ECB;
            Crypto.Padding = padding;

            var Key = new byte[KeySize];
            fixed (byte* pKey = Key, pkey = key)
            {
                byte* ps = pkey, pd = pKey;
                for (int i = 0; i < KeySize; i += 4, ps += 4, pd += 4)
                    *((int*)pd) = *((int*)ps);
            }

            CryptoTransform = Mode == CipherMode.Encryption ? Crypto.CreateEncryptor(Key, null) : Crypto.CreateDecryptor(Key, null);
        }

        public override void Transform(byte[] input)
        {
            CryptoTransform.TransformBlock(input, 0, input.Length, input, 0);
        }

        public override byte[] TransformFinal(byte[] input)
        {
            return CryptoTransform.TransformFinalBlock(input, 0, input.Length);
        }
    }
}
