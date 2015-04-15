using System.Linq;
using System.Security.Cryptography;
using SSH.Encryption;
using SSH.IO;
using SSH.Processor;

namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_KEXINIT)]
    public class SshKeyExchangeInit : Packet
    {
        [SshProperty(1, Raw = true, RawLength = 16)]
        public byte[] Cookie { get; set; }
        [SshProperty(2)]
        public string[] KexAlgorithms { get; set; }
        [SshProperty(3)]
        public string[] KeyAlgorithmsServer { get; set; }
        [SshProperty(4)]
        public string[] EncAlgorithmsClient { get; set; }
        [SshProperty(5)]
        public string[] EncAlgorithmsServer { get; set; }
        [SshProperty(6)]
        public string[] MacAlgorithmsClient { get; set; }
        [SshProperty(7)]
        public string[] MacAlgorithmsServer { get; set; }
        [SshProperty(8)]
        public string[] ComAlgorithmsClient { get; set; }
        [SshProperty(9)]
        public string[] ComAlgorithmsServer { get; set; }
        [SshProperty(10)]
        public string[] LanguagesClient { get; set; }
        [SshProperty(11)]
        public string[] LanguagesServer { get; set; }
        [SshProperty(12, Raw = true, RawLength = 5)]
        public byte[] Reserved { get { return new byte[5]; } set { } }

        public SshKeyExchangeInit()
            : base(MessageCode.SSH_MSG_KEXINIT)
        {
            var r = new RNGCryptoServiceProvider();
            this.Cookie = r.GetBytes(16);
            this.KexAlgorithms = KeyExchangeProcessor.Algorithms;
            this.KeyAlgorithmsServer = new string[] { "ecdsa-sha2-nistp256", "ssh-rsa", "ssh-dss" };
            this.EncAlgorithmsClient = Cipher.Algorithms;
            this.EncAlgorithmsServer = EncAlgorithmsClient;
            this.MacAlgorithmsClient = MacWriter.Algorithms;
            this.MacAlgorithmsServer = MacAlgorithmsClient;
            this.ComAlgorithmsClient = new string[] { "none" }; //"zlib", "zlib@openssh.com" };
            this.ComAlgorithmsServer = ComAlgorithmsClient;
            this.LanguagesClient = LanguagesServer = new string[] { };
        }

        public SshKeyExchangeInit(SshKeyExchangeInit server)
            : this()
        {
            this.KexAlgorithms = Guess(server.KexAlgorithms, this.KexAlgorithms);
            this.KeyAlgorithmsServer = Guess(server.KeyAlgorithmsServer, this.KeyAlgorithmsServer);
            this.EncAlgorithmsClient = Guess(server.EncAlgorithmsClient, this.EncAlgorithmsClient);
            this.EncAlgorithmsServer = Guess(server.EncAlgorithmsServer, this.EncAlgorithmsServer);
            this.MacAlgorithmsClient = Guess(server.MacAlgorithmsClient, this.MacAlgorithmsClient);
            this.MacAlgorithmsServer = Guess(server.MacAlgorithmsServer, this.MacAlgorithmsServer);
            this.ComAlgorithmsClient = Guess(server.ComAlgorithmsClient, this.ComAlgorithmsClient);
            this.ComAlgorithmsServer = Guess(server.ComAlgorithmsServer, this.ComAlgorithmsServer);
            this.LanguagesClient = Guess(server.LanguagesClient, this.LanguagesClient);
            this.LanguagesServer = Guess(server.LanguagesServer, this.LanguagesServer);
        }

        private string[] Guess(string[] server, string[] client)
        {
            var s = server.ToList();
            var c = (from s1 in client where s.Contains(s1) select s1).ToList();
            c.Sort((s1, s2) => c.IndexOf(s1).CompareTo(c.IndexOf(s2)));
            return c.ToArray();
        }
    }
}
