using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SSH.DiffieHellman;
using SSH.IO;
using SSH.Processor;
using System.Reflection;
using SSH.Attributes;

namespace SSH.Encryption
{
    abstract class Cipher
    {
        private static CipherAttribute[] types;

        public CipherMode Mode { get; set; }
        public virtual int BlockSize { get { return 0; } }
        public virtual int KeySize { get { return 0; } }

        internal virtual Type CryptoType { get; private set; }

        internal virtual SymmetricAlgorithm Crypto { get; set; }
        internal virtual ICryptoTransform Cipher2 { get; set; }

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

        public static Cipher Create(string cipher, CipherMode mode, Type hashType, byte[] sharedSecret, byte[] exchangeHash, byte[] key, byte[] iv)
        {
            Cipher c = Create(cipher);

            if (c != null)
            {
                var key2 = KeyExchangeProcessor.ExpandKey(key, sharedSecret, exchangeHash, c.KeySize, hashType);
                var iv2 = KeyExchangeProcessor.ExpandKey(iv, sharedSecret, exchangeHash, c.BlockSize, hashType);
                c.Initialize(mode, key2, iv2);
            }
            return c;
        }

        public static Cipher Create(string cipher)
        {
            return (Cipher)Activator.CreateInstance(types.Where(a => a.Name == cipher).Single().Type);
        }

        public static string[] Algorithms
        {
            get
            {
                return types.Where(a => a.Negotiable).Select(a => a.Name).ToArray();
            }
        }
    }
}
