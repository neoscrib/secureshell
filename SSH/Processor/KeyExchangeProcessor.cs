using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SSH.Attributes;
using SSH.Encryption;
using SSH.IO;
using SSH.Packets;
using System.Reflection;

namespace SSH.Processor
{
    class KeyExchangeProcessor : PacketProcessor, IPacketProcessor
    {
        private static KeyExchangeAttribute[] types;

        public SshKeyExchangeInit ServerKexInit { get; set; }
        public SshKeyExchangeInit ClientKexInit { get; set; }

        internal IKeyExchangeProcessor kexProcessor;
        private EventWaitHandle waitHandle2;

        static KeyExchangeProcessor()
        {
            types = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(KeyExchangeAttribute), false).
                Cast<KeyExchangeAttribute>().Where(a => typeof(IKeyExchangeProcessor).IsAssignableFrom(a.Type)).
                OrderBy(a => a.Priority).ToArray();
        }

        public KeyExchangeProcessor(Session session)
            : base(session)
        {
            waitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_KEXINIT:
                    ServerKexInit = (SshKeyExchangeInit)p;
                    ClientKexInit = new SshKeyExchangeInit(ServerKexInit);
                    kexProcessor = KeyExchangeProcessor.Create(ClientKexInit.KexAlgorithms[0], session);
                    waitHandle2.Set();
                    session.Socket.WritePacket(ClientKexInit);
                    kexProcessor.Initialize();
                    return true;
                case MessageCode.SSH_MSG_NEWKEYS:
                    UpdateKeys();
                    session.Socket.WritePacket(new SshNewKeys());
                    Close(SSH.StatusCode.OK);
                    return true;
            }

            return false;
        }

        private void UpdateKeys()
        {
            if (session.SessionId == null)
                session.SessionId = kexProcessor.ExchangeHash;

            Debug.WriteLine(string.Format("Shared Secret: {0}", kexProcessor.SharedSecret.HexDump()));
            for (char c = 'A'; c <= 'F'; c++)
            {
                using (var hash = HashWriter.Create(ClientKexInit.KexAlgorithms[0]))
                {
                    hash.WriteString(kexProcessor.SharedSecret);
                    hash.Write(kexProcessor.ExchangeHash);
                    hash.Write(c);
                    hash.Write(session.SessionId);
                    session.Keys[c - 'A'] = hash.Hash;
                    Debug.WriteLine("Key '{0}': {1}", c, hash.Hash.HexDump());
                }
            }

            var hashType = HashWriter.GetType(ClientKexInit.KexAlgorithms[0]);
            session.Socket.Encryptor = Cipher.Create(ClientKexInit.EncAlgorithmsClient[0], Cipher.CipherMode.Encryption,
                hashType, kexProcessor.SharedSecret, kexProcessor.ExchangeHash, session.Keys[2], session.Keys[0]);
            session.Socket.Decryptor = Cipher.Create(ClientKexInit.EncAlgorithmsServer[0], Cipher.CipherMode.Decryption,
                hashType, kexProcessor.SharedSecret, kexProcessor.ExchangeHash, session.Keys[3], session.Keys[1]);
            session.Socket.EncryptorMac = new Mac.MacActivator(ClientKexInit.MacAlgorithmsClient[0],
                hashType, kexProcessor.SharedSecret, kexProcessor.ExchangeHash, session.Keys[4]);
            session.Socket.DecryptorMac = new Mac.MacActivator(ClientKexInit.MacAlgorithmsServer[0],
                hashType, kexProcessor.SharedSecret, kexProcessor.ExchangeHash, session.Keys[5]);
        }

        public static byte[] ExpandKey(byte[] key, byte[] sharedSecret, byte[] exchangeHash, int desiredLength, Type hashType)
        {
            var key2 = new byte[key.Length];
            Buffer.BlockCopy(key, 0, key2, 0, key.Length);

            while (key2.Length < desiredLength)
            {
                using (var hw = (HashWriter)Activator.CreateInstance(hashType))
                {
                    hw.WriteString(sharedSecret);
                    hw.Write(exchangeHash);
                    hw.Write(key2);
                    key2 = key2.Concat(hw.Hash).ToArray();
                }
            }

            if (key2.Length > desiredLength)
                key2 = key2.Take(desiredLength).ToArray();
            return key2;
        }

        public new void Wait()
        {
            waitHandle2.WaitOne();
            kexProcessor.Wait();
            if (kexProcessor.StatusCode != SSH.StatusCode.OK)
                Close(kexProcessor.StatusCode);
            else
                base.Wait();
        }

        internal static IKeyExchangeProcessor Create(string name, params object[] parameters)
        {
            return (IKeyExchangeProcessor)Activator.CreateInstance(
                types.Where(a => a.Name == name).Single().Type, parameters);
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
