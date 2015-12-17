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

        public void Reset()
        {
            algorithm.Initialize();
        }

        public virtual byte[] Hash
        {
            get
            {
                algorithm.TransformFinalBlock(new byte[0], 0, 0);
                return algorithm.Hash;
            }
        }

        public bool IsMatch(byte[] mac)
        {
            var hash = this.Hash;
            if (mac.Length != BlockSize || hash.Length != BlockSize)
                return false;
            for (var i = 0; i < BlockSize; i++)
            {
                if (mac[i] != hash[i])
                    return false;
            }
            return true;
        }

        public static MacWriter Create(string name, byte[] key)
        {
            var t = types.Where(a => a.Name == name).Single().Type;
            return (MacWriter)Activator.CreateInstance(t, key);
        }

        public static MacWriter Create(string name)
        {
            var t = types.Where(a => a.Name == name).Single().Type;
            return (MacWriter)Activator.CreateInstance(t);
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
