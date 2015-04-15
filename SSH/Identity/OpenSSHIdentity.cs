using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SSH.Encryption;

namespace SSH.Identity
{
    public abstract class OpenSSHIdentity : IdentityFile
    {
        internal byte[] blob;
        Cipher cipher;
        byte[] iv;

        public abstract override byte[] PublicKey { get; }

        public abstract override void ExtractParameters();

        public abstract override byte[] Sign(byte[] data);

        public OpenSSHIdentity(string path)
            : base(path)
        {
            this.blob = Process();
        }

        public override byte[] Process()
        {
            using (var sr = new StreamReader(Path))
            {
                string line = sr.ReadLine();
                string b64 = string.Empty;
                while (!(line = sr.ReadLine()).StartsWith("-----"))
                {
                    while (line.TrimEnd().EndsWith("\\"))
                        line = sr.ReadLine();
                    if (line.Contains(":"))
                    {
                        if (line.StartsWith("DEK-Info"))
                        {
                            Encrypted = true;
                            var kvp = line.Split(':');
                            kvp[1] = kvp[1].Trim();
                            var items = kvp[1].Split(',');
                            items[0] = items[0].ToLower();
                            cipher = Cipher.Create(items[0]);
                            iv = BigInteger.Parse(items[1], System.Globalization.NumberStyles.AllowHexSpecifier).ToByteArray().Reverse().ToArray();
                        }
                        continue;
                    }
                    b64 += line;
                }
                var blob = Convert.FromBase64String(b64);
                return blob;
            }
        }

        public override void Decrypt()
        {
            if (Encrypted)
            {
                var passphrase = PassphraseFunction(this);
                var key = DeriveKey(passphrase, iv, (uint)cipher.KeySize);
                cipher.Initialize(Cipher.CipherMode.Decryption, key, iv, PaddingMode.PKCS7);
                blob = cipher.TransformFinal(blob);
                if (blob[0] != 0x30)
                    throw new CryptographicException("Private key decryption failed.");

                Encrypted = false;
            }
            ExtractParameters();
        }

        protected static byte[] DeriveKey(string passphrase, byte[] iv, uint keySize)
        {
            byte[] iv2 = iv.Take(8).ToArray();
            byte[] passphraseBytes = passphrase.ToByteArray();
            var md5 = new MD5CryptoServiceProvider();
            var hash = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write);
            byte[] key = new byte[keySize];
            uint hashesSize = keySize & 0xfffffff0;
            uint MD5_HASH_BYTES = (uint)(md5.HashSize / 8);
            if ((keySize & 0xf) != 0)
                hashesSize += MD5_HASH_BYTES;
            byte[] hashes = new byte[hashesSize];
            byte[] previous;
            for (int index = 0; (index + MD5_HASH_BYTES) <= hashes.Length; hash.Write(previous, 0, previous.Length))
            {
                hash.Write(passphraseBytes, 0, passphraseBytes.Length);
                hash.Write(iv2, 0, iv2.Length);
                hash.FlushFinalBlock();
                previous = md5.Hash;
                md5 = new MD5CryptoServiceProvider();
                hash = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write);
                Buffer.BlockCopy(previous, 0, hashes, index, previous.Length);
                index += previous.Length;
            }
            Buffer.BlockCopy(hashes, 0, key, 0, key.Length);
            return key;
        }
    }
}
