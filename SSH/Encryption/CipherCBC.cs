using System;
using System.Security.Cryptography;
using System.Linq;

namespace SSH.Encryption
{
    abstract class CipherCBC : Cipher
    {
        public override void Initialize(CipherMode mode, byte[] key, byte[] iv, PaddingMode padding = PaddingMode.None)
        {
            Mode = mode;
            Crypto =  (SymmetricAlgorithm)Activator.CreateInstance(CryptoType);
            Crypto.BlockSize = BlockSize * 8;
            Crypto.Mode = System.Security.Cryptography.CipherMode.CBC;
            Crypto.Padding = padding;

            var IV = iv.Take(BlockSize).ToArray();
            var Key = key.Take(KeySize).ToArray();

            CryptoTransform = Mode == CipherMode.Encryption ? 
                Crypto.CreateEncryptor(Key, IV) : 
                Crypto.CreateDecryptor(Key, IV);
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
