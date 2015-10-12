using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;

namespace SSH.Identity
{
    public abstract class IdentityFile : IIdentityFile
    {
        private static Dictionary<string, Type> types;

        public string Path { get; set; }
        public bool Encrypted { get; set; }
        public AsymmetricAlgorithm Algorithm { get; set; }

        public string AlgorithmName
        {
            get
            {
                Decrypt();
                return Algorithm is RSA ? "ssh-rsa" :
                    Algorithm is DSA ? "ssh-dss" :
                    Algorithm.SignatureAlgorithm;
            }
        }

        public Func<IIdentityFile, SecureString> PassphraseFunction { get; set; }

        public abstract byte[] PublicKey { get; }

        public abstract byte[] Process();

        public abstract void Decrypt();

        public abstract void ExtractParameters();

        public abstract byte[] Sign(byte[] data);

        static IdentityFile()
        {
            types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).
                Where(t => typeof(IIdentityFile).IsAssignableFrom(t) && t.IsDefined(typeof(IdentityFileAttribute), false)).
                SelectMany(t => t.GetCustomAttributes(typeof(IdentityFileAttribute), false).
                    Cast<IdentityFileAttribute>().
                    Select(a => new { a, t })).
                ToDictionary(ta => ta.a.Name, ta => ta.t);
        }

        public IdentityFile(string path)
        {
            this.Path = path;
            this.PassphraseFunction = Session.ConsolePassphraseFunction;
        }

        public static IIdentityFile Create(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string line = sr.ReadLine();
                if (types.ContainsKey(line))
                    return (IIdentityFile)Activator.CreateInstance(types[line], path);
                else
                    throw new CryptographicException("Only RSA, DSA and ECDSA in OpenSSH and PuTTY format are supported.");
            }
        }

        public class IdentityFileAttribute : Attribute
        {
            public string Name { get; set; }

            public IdentityFileAttribute(string name)
            {
                this.Name = name;
            }
        }
    }
}
