using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SSH.IO
{
    abstract class HashWriter : PacketWriter
    {
        private static Dictionary<string, Type> types;

        protected HashAlgorithm algorithm;
        protected string name;

        static HashWriter()
        {
            types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).
                Where(t => typeof(HashWriter).IsAssignableFrom(t) && t.IsDefined(typeof(HashWriterAttribute), false)).
                SelectMany(t => t.GetCustomAttributes(typeof(HashWriterAttribute), false).
                    Cast<HashWriterAttribute>().
                    Select(a => new { a, t })).
                ToDictionary(ta => ta.a.Name, ta => ta.t);
        }

        protected HashWriter(HashAlgorithm h)
            : base(new CryptoStream(System.IO.Stream.Null, h, CryptoStreamMode.Write))
        {
            algorithm = h;
        }

        public virtual byte[] Hash
        {
            get
            {
                algorithm.TransformFinalBlock(new byte[0], 0, 0);
                return algorithm.Hash;
            }
        }

        public HashAlgorithm Algorithm
        {
            get { return algorithm; }
        }

        public string AlgorithmName
        {
            get { return name; }
        }

        public static HashWriter Create(string name)
        {
            var type = types[name];
            return (HashWriter)Activator.CreateInstance(type);
        }

        public static Type GetType(string name)
        {
            return types[name];
        }

        public void Reset()
        {
            algorithm.Initialize();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class HashWriterAttribute : Attribute
    {
        public string Name { get; set; }

        public HashWriterAttribute(string name)
        {
            this.Name = name;
        }
    }
}
