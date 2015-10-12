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
            waitHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_KEXINIT:
                    ServerKexInit = (SshKeyExchangeInit)p;
                    ServerKexInit.UseCompression = session.UseCompression;
                    ClientKexInit = new SshKeyExchangeInit(ServerKexInit);
                    if (session.UseCompression)
                        ClientKexInit.ComAlgorithmsClient = new string[] { "zlib@openssh.com", "zlib", "none" };
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

            var a1 = Cipher.Create(ClientKexInit.EncAlgorithmsClient[0]);
            var a2 = Cipher.Create(ClientKexInit.EncAlgorithmsServer[0]);
            var a3 = MacWriter.Create(ClientKexInit.MacAlgorithmsClient[0]);
            var a4 = MacWriter.Create(ClientKexInit.MacAlgorithmsServer[0]);
            var desiredLengths = new int[] { a1.BlockSize, a2.BlockSize, a1.KeySize,
                a2.KeySize, a3.KeySize, a4.KeySize };

            Debug.WriteLine(string.Format("Shared Secret: {0}", kexProcessor.SharedSecret.HexDump()));
            byte[][] keys = new byte[6][];
            using (var hash = HashWriter.Create(ClientKexInit.KexAlgorithms[0]))
            {
                var i = 0;
                for (char c = 'A'; c <= 'F'; c++, i++)
                {
                    hash.WriteString(kexProcessor.SharedSecret);
                    hash.Write(kexProcessor.ExchangeHash);
                    hash.Write(c);
                    hash.Write(session.SessionId);
                    var key = hash.Hash;
                    hash.Reset();
                    while (key.Length < desiredLengths[i])
                    {
                        hash.WriteString(kexProcessor.SharedSecret);
                        hash.Write(kexProcessor.ExchangeHash);
                        hash.Write(key);
                        var h = hash.Hash;
                        var buffer = new byte[key.Length + h.Length];
                        Buffer.BlockCopy(key, 0, buffer, 0, key.Length);
                        Buffer.BlockCopy(h, 0, buffer, key.Length, h.Length);
                        key = buffer;
                        Extensions.Random.ClearBytes(buffer, 0, buffer.Length);
                        hash.Reset();
                    }

                    keys[i] = new byte[desiredLengths[i]];
                    Buffer.BlockCopy(key, 0, keys[i], 0, desiredLengths[i]);
                    Extensions.Random.ClearBytes(key, 0, key.Length);
                    Debug.WriteLine("Key '{0}': {1}", c, keys[i].HexDump());
                }
            }

            a1.Initialize(Cipher.CipherMode.Encryption, keys[2], keys[0]);
            a2.Initialize(Cipher.CipherMode.Decryption, keys[3], keys[1]);
            session.Socket.Encryptor = a1;
            session.Socket.Decryptor = a2;
            session.Socket.EncryptorMac = MacWriter.Create(ClientKexInit.MacAlgorithmsClient[0], keys[4]);
            session.Socket.DecryptorMac = MacWriter.Create(ClientKexInit.MacAlgorithmsServer[0], keys[5]);

            for (var i = 0; i < keys.Length; i++)
            {
                Extensions.Random.ClearBytes(keys[i], 0, keys[i].Length);
            }
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
