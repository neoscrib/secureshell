using System;
using System.Security.Cryptography;
using System.Linq;
using System.Diagnostics;

namespace SSH.Encryption
{
    abstract class CipherCTR : Cipher
    {
        private byte[] x;

        public override void Initialize(CipherMode mode, byte[] key, byte[] iv, PaddingMode padding = PaddingMode.None)
        {
            Mode = mode;
            Crypto = (SymmetricAlgorithm)Activator.CreateInstance(CryptoType);
            Crypto.BlockSize = BlockSize * 8;
            Crypto.KeySize = KeySize * 8;
            Crypto.Mode = System.Security.Cryptography.CipherMode.ECB;
            Crypto.Padding = padding;

            var IV = iv.Take(BlockSize).ToArray();
            var Key = key.Take(KeySize).ToArray();
            x = iv.Take(BlockSize).ToArray();

            CryptoTransform = Crypto.CreateEncryptor(Key, null);
        }

        public unsafe override void Transform(byte[] input)
        {
            byte[] buffer = new byte[BlockSize];
            for (int i = 0; i < input.Length; i += BlockSize)
            {
                CryptoTransform.TransformBlock(x, 0, BlockSize, buffer, 0);
                input.Xor(i, buffer, 0, BlockSize);
                x.Increment();
            }
        }

        public override unsafe byte[] TransformFinal(byte[] input)
        {
            Transform(input);
            return input;
        }
    }
}
