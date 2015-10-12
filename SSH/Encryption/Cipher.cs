using SSH.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace SSH.Encryption
{
    abstract class Cipher : ICryptoTransform
    {
        private static CipherAttribute[] types;

        public CipherMode Mode { get; set; }
        public virtual int BlockSize { get { return 0; } }
        public virtual int KeySize { get { return 0; } }

        internal virtual ICryptoTransform CryptoTransform { get; set; }
        internal virtual Type CryptoType { get; private set; }
        internal virtual SymmetricAlgorithm Crypto { get; set; }

        public static string[] Algorithms { get { return types.Where(a => a.Negotiable).Select(a => a.Name).ToArray(); } }

        static Cipher()
        {
            types = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(CipherAttribute), false).
                Cast<CipherAttribute>().Where(a => typeof(Cipher).IsAssignableFrom(a.Type)).
                OrderBy(a => a.Priority).ToArray();
        }

        public enum CipherMode
        {
            Encryption,
            Decryption
        }

        public abstract void Initialize(CipherMode mode, byte[] key, byte[] iv, PaddingMode padding = PaddingMode.None);
        public abstract void Transform(byte[] input);
        public abstract byte[] TransformFinal(byte[] input);

        public static Cipher Create(string cipher)
        {
            return (Cipher)Activator.CreateInstance(types.Where(a => a.Name == cipher).Single().Type);
        }

        // ICryptoTransform implementation

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var buffer = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, buffer, 0, inputCount);
            Transform(buffer);
            Buffer.BlockCopy(buffer, 0, outputBuffer, outputOffset, inputCount);
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var buffer = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, buffer, 0, inputCount);
            return TransformFinal(buffer);
        }

        public void Dispose()
        {
            CryptoTransform.Dispose();
        }

        public int InputBlockSize { get { return CryptoTransform.InputBlockSize; } }
        public int OutputBlockSize { get { return CryptoTransform.OutputBlockSize; } }
        public bool CanTransformMultipleBlocks { get { return CryptoTransform.CanTransformMultipleBlocks; } }
        public bool CanReuseTransform { get { return CryptoTransform.CanReuseTransform; } }
    }
}
