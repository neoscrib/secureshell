using System;
using System.IO;
using System.Security.Cryptography;
using SSH.Encryption;
using SSH.IO;
using System.Security;

namespace SSH.Identity
{
    public abstract class PuttyIdentity : IdentityFile
    {
        internal byte[] blob;
        Cipher cipher;
        byte[] iv;

        public PuttyIdentity(string path)
            : base(path)
        { }

        public abstract override byte[] PublicKey { get; }

        public override byte[] Process()
        {
            using (var sr = new StreamReader(Path))
            {
                string line = sr.ReadLine();
                string b64 = string.Empty;
                byte[] publickey = new byte[0];
                byte[] privatekey = new byte[0];
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(":"))
                    {
                        if (line.StartsWith("Encryption:"))
                        {
                            var enc = line.Split(':')[1].Trim();
                            Encrypted = enc != "none";
                            if (Encrypted) cipher = Cipher.Create(enc);
                            iv = new byte[cipher.BlockSize];
                        }
                        if (line.StartsWith("Public-Lines:"))
                        {
                            var lines = int.Parse(line.Split(':')[1]);
                            for (int i = 0; i < lines; i++)
                                b64 += sr.ReadLine();
                            publickey = Convert.FromBase64String(b64);
                            b64 = string.Empty;
                        }
                        if (line.StartsWith("Private-Lines:"))
                        {
                            var lines = int.Parse(line.Split(':')[1]);
                            for (int i = 0; i < lines; i++)
                                b64 += sr.ReadLine();
                            privatekey = Convert.FromBase64String(b64);
                            b64 = string.Empty;
                        }
                    }
                }
                var blob = new byte[publickey.Length + privatekey.Length];
                Buffer.BlockCopy(publickey, 0, blob, 0, publickey.Length);
                Buffer.BlockCopy(privatekey, 0, blob, publickey.Length, privatekey.Length);
                return blob;
            }
        }

        public override void Decrypt()
        {
            if (blob == null)
                blob = Process();

            if (Encrypted)
            {
                var passphrase = PassphraseFunction(this);
                using (var pr = new PacketReader(blob))
                {
                    var type = pr.ReadStringAsString();
                    int count = type == "ssh-rsa" ? 2 : 4;
                    for (int i = 0; i < count; i++)
                        pr.ReadString();
                    var length = pr.Length - pr.Position;
                    byte[] ciphertext = new byte[length];
                    Buffer.BlockCopy(blob, (int)pr.Position, ciphertext, 0, (int)length);
                    var key = DeriveKey(passphrase, (uint)cipher.KeySize);
                    cipher.Initialize(Cipher.CipherMode.Decryption, key, iv, PaddingMode.None);

                    var plaintext = cipher.TransformFinal(ciphertext);
                    TestPlaintext(type, plaintext);
                    Buffer.BlockCopy(plaintext, 0, blob, (int)pr.Position, plaintext.Length);
                }

                Encrypted = false;
            }
            ExtractParameters();
        }

        private static void TestPlaintext(string type, byte[] plaintext)
        {
            using (var pr1 = new PacketReader(plaintext))
            {
                try
                {
                    if (type == "ssh-dss")
                    {
                        pr1.ReadString();
                    }
                    else if (type == "ssh-rsa")
                    {
                        pr1.ReadString();
                        pr1.ReadString();
                        pr1.ReadString();
                        pr1.ReadString();
                    }
                }
                catch (Exception)
                {
                    throw new CryptographicException("Private key decryption failed.");
                }
            }
        }

        protected static byte[] DeriveKey(SecureString passphrase, uint keySize)
        {
            byte[] key = new byte[keySize];

            passphrase.Consume(d =>
            {
                byte[] data = new byte[4 + d.Length];
                Buffer.BlockCopy(d, 0, data, 4, d.Length);
                var sha = new SHA1CryptoServiceProvider();
                byte[] hash = sha.ComputeHash(data);
                Buffer.BlockCopy(hash, 0, key, 0, hash.Length);
                data[3] = 0x01;
                hash = sha.ComputeHash(data);
                Extensions.Random.ClearBytes(data, 0, data.Length);
                Buffer.BlockCopy(hash, 0, key, hash.Length, key.Length - hash.Length);
            });
            return key;
        }

        public abstract override void ExtractParameters();

        public abstract override byte[] Sign(byte[] data);
    }
}
