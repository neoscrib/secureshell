using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using SSH.Attributes;
using SSH.Processor;

namespace SSH.IO
{
    abstract class MacWriter : PacketWriter
    {
        private static MacWriterAttribute[] types;

        protected HashAlgorithm algorithm;

        public virtual int KeySize { get { return 0; } }
        public virtual int BlockSize { get { return 0; } }

        static MacWriter()
        {
            types = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(MacWriterAttribute), false).
                Cast<MacWriterAttribute>().Where(a => typeof(MacWriter).IsAssignableFrom(a.Type)).
                OrderBy(a => a.Priority).ToArray();
        }

        protected MacWriter() : base(Stream.Null) { }

        protected MacWriter(HashAlgorithm h)
            : base(new CryptoStream(Stream.Null, h, CryptoStreamMode.Write))
        {
            algorithm = h;
        }

        public virtual byte[] Hash
        {
            get
            {
                base.Close();
                return algorithm.Hash;
            }
        }

        public static MacWriter Create(string name, Type hashType, byte[] sharedSecret, byte[] exchangeHash, byte[] key)
        {
            var t = types.Where(a => a.Name == name).Single().Type;
            var temp = (MacWriter)Activator.CreateInstance(t);
            var keySize = temp.KeySize;
            var key2 = KeyExchangeProcessor.ExpandKey(key, sharedSecret, exchangeHash, keySize, hashType);

            return (MacWriter)Activator.CreateInstance(t, key2);
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
