using System;
using SSH.Attributes;
using SSH.Encryption;
using SSH.Mac;
using SSH.Processor;

[assembly: KeyExchange("ecdh-sha2-nistp521", Type = typeof(ECDHProcessor), Priority = 0, Negotiable = true)]
[assembly: KeyExchange("ecdh-sha2-nistp384", Type = typeof(ECDHProcessor), Priority = 1, Negotiable = true)]
[assembly: KeyExchange("ecdh-sha2-nistp256", Type = typeof(ECDHProcessor), Priority = 2, Negotiable = true)]
[assembly: KeyExchange("diffie-hellman-group-exchange-sha256", Type = typeof(DHGEProcessor), Priority = 3, Negotiable = true)]
[assembly: KeyExchange("diffie-hellman-group-exchange-sha1", Type = typeof(DHGEProcessor), Priority = 4, Negotiable = true)]
[assembly: KeyExchange("diffie-hellman-group14-sha1", Type = typeof(DHGProcessor), Priority = 5, Negotiable = true)]
[assembly: KeyExchange("diffie-hellman-group1-sha1", Type = typeof(DHGProcessor), Priority = 6, Negotiable = true)]

[assembly: Cipher("aes256-ctr", Type = typeof(AES256_CTR), Priority = 0, Negotiable = true)]
[assembly: Cipher("aes192-ctr", Type = typeof(AES192_CTR), Priority = 1, Negotiable = true)]
[assembly: Cipher("aes128-ctr", Type = typeof(AES128_CTR), Priority = 2, Negotiable = true)]
[assembly: Cipher("aes256-cbc", Type = typeof(AES256_CBC), Priority = 3, Negotiable = true)]
[assembly: Cipher("aes-256-cbc", Type = typeof(AES256_CBC), Priority = 3, Negotiable = false)]
[assembly: Cipher("rijndael-cbc@lysator.liu.se", Type = typeof(AES256_CBC), Priority = 3, Negotiable = true)]
[assembly: Cipher("aes192-cbc", Type = typeof(AES192_CBC), Priority = 4, Negotiable = true)]
[assembly: Cipher("aes-192-cbc", Type = typeof(AES192_CBC), Priority = 4, Negotiable = false)]
[assembly: Cipher("aes128-cbc", Type = typeof(AES128_CBC), Priority = 5, Negotiable = true)]
[assembly: Cipher("aes-128-cbc", Type = typeof(AES128_CBC), Priority = 5, Negotiable = false)]
[assembly: Cipher("3des-ctr", Type = typeof(TripleDES_CTR), Priority = 6, Negotiable = true)]
[assembly: Cipher("3des-cbc", Type = typeof(TripleDES_CBC), Priority = 7, Negotiable = true)]
[assembly: Cipher("des-ede3-cbc", Type = typeof(TripleDES_CBC), Priority = 7, Negotiable = false)]
[assembly: Cipher("cast128-cbc", Type = typeof(Cast128_CBC), Priority = 8, Negotiable = true)]
[assembly: Cipher("blowfish-cbc", Type = typeof(Blowfish_CBC), Priority = 9, Negotiable = true)]
[assembly: Cipher("3des-ecb", Type = typeof(TripleDES_ECB), Priority = 10, Negotiable = false)]

[assembly: MacWriter("hmac-sha2-512", Type = typeof(HMACSHA512), Priority = 0, Negotiable = false)]
[assembly: MacWriter("hmac-sha2-384", Type = typeof(HMACSHA384), Priority = 1, Negotiable = false)]
[assembly: MacWriter("hmac-sha2-256", Type = typeof(HMACSHA256), Priority = 2, Negotiable = true)]
[assembly: MacWriter("hmac-ripemd160", Type = typeof(HMACRIPEMD160), Priority = 3, Negotiable = true)]
[assembly: MacWriter("hmac-ripemd160@openssh.com", Type = typeof(HMACRIPEMD160), Priority = 3, Negotiable = true)]
[assembly: MacWriter("hmac-sha1", Type = typeof(HMACSHA1), Priority = 4, Negotiable = true)]
[assembly: MacWriter("hmac-md5", Type = typeof(HMACMD5), Priority = 5, Negotiable = true)]
[assembly: MacWriter("hmac-sha1-96", Type = typeof(HMACSHA196), Priority = 6, Negotiable = true)]
[assembly: MacWriter("hmac-md5-96", Type = typeof(HMACMD596), Priority = 7, Negotiable = true)]

namespace SSH.Attributes
{
    internal abstract class AlgorithmAttribute : Attribute
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public int Priority { get; set; }
        public bool Negotiable { get; set; }

        public AlgorithmAttribute(string name)
        {
            this.Name = name;
            this.Negotiable = true;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class CipherAttribute : AlgorithmAttribute
    {
        public CipherAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class KeyExchangeAttribute : AlgorithmAttribute
    {
        public KeyExchangeAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class MacWriterAttribute : AlgorithmAttribute
    {
        public MacWriterAttribute(string name) : base(name) { }
    }
}
