using System;
using System.Collections.Generic;
using System.Linq;
using SSH.IO;
using SSH.Processor;

namespace SSH.Packets
{
    class TypePair<T> where T : Attribute
    {
        public T Attribute { get; set; }
        public Type Type { get; set; }

        public TypePair(T attribute, Type type)
        {
            this.Attribute = attribute;
            this.Type = type;
        }
    }

    public static class PacketFactory
    {
        static Dictionary<SftpMessageCode, List<Type>> typeLookupSftp;
        static Dictionary<MessageCode, List<Type>> typeLookup;
        static Dictionary<MessageCode, Func<Session, byte[], Type>> tieBreakers;

        static PacketFactory()
        {
            var typePairs = GetTypePairs<PacketAttribute>();
            typeLookup = new Dictionary<MessageCode, List<Type>>();
            foreach (var ta in typePairs)
            {
                if (!typeLookup.ContainsKey(ta.Attribute.MessageCode))
                    typeLookup.Add(ta.Attribute.MessageCode, new List<Type>());
                typeLookup[ta.Attribute.MessageCode].Add(ta.Type);
            }

            var typePairsSftp = GetTypePairs<SftpPacketAttribute>();
            typeLookupSftp = new Dictionary<SftpMessageCode, List<Type>>();
            foreach (var ta in typePairsSftp)
            {
                if (!typeLookupSftp.ContainsKey(ta.Attribute.MessageCode))
                    typeLookupSftp.Add(ta.Attribute.MessageCode, new List<Type>());
                typeLookupSftp[ta.Attribute.MessageCode].Add(ta.Type);
            }

            tieBreakers = new Dictionary<MessageCode, Func<Session, byte[], Type>>();

            AddTieBreaker((MessageCode)31, SelectorKexCode);
            AddTieBreaker(MessageCode.SSH_MSG_CHANNEL_REQUEST, SelectorSshChannelRequest);
            AddTieBreaker(MessageCode.SSH_MSG_CHANNEL_DATA, SelectorSshChannelData);
            AddTieBreaker(MessageCode.SSH_MSG_CHANNEL_OPEN, SelectorChannelOpen);
        }

        private static IEnumerable<TypePair<T>> GetTypePairs<T>() where T : Attribute
        {
            var typePairs = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).
                Where(t => typeof(IPacket).IsAssignableFrom(t) && t.IsDefined(typeof(T), false)).
                SelectMany(t => t.GetCustomAttributes(typeof(T), false).
                    Cast<T>().Select(a => new TypePair<T>(a, t)));
            return typePairs;
        }

        public static void AddTieBreaker(MessageCode code, Func<Session, byte[], Type> selector)
        {
            tieBreakers.Add(code, selector);
        }

        internal static IPacket Build<T>(byte[] data, Type type) where T : IPacket, new()
        {
            return new T();
        }

        internal static IPacket Build(byte[] data, Type type)
        {
            var p = (IPacket)Activator.CreateInstance(type);
            var props = Packet.GetProperties(type);
            using (var pr = new PacketReader(data))
            {
                for (int i = 0; i < props.Length; i++)
                {
                    props[i].Information.SetValue(p, pr.ReadProperty(props[i]), null);
                }
            }
            return p;
        }

        internal static IPacket BuildSftp(byte[] data, Type type)
        {
            var p = (IPacket)Activator.CreateInstance(type);
            var props = Packet.GetProperties(type);
            using (var pr = new PacketReader(data))
            {
                for (int i = 0; i < props.Length; i++)
                {
                    props[i].Information.SetValue(p, pr.ReadProperty(props[i]), null);
                    if (i == 1)
                    {
                        pr.ReadUInt32(); pr.ReadUInt32();
                    }
                }
            }
            return p;
        }

        public static IPacket Create(Session session, byte[] data)
        {
            IPacket p = null;
            MessageCode code = (MessageCode)data[0];
            if (typeLookup.ContainsKey(code))
            {
                var types = typeLookup[code];
                Type type;
                if (tieBreakers.ContainsKey(code))
                    type = tieBreakers[code](session, data);
                else
                    type = types.First();

                if (type != null)
                {
                    if (type.Equals(typeof(SftpPacket)))
                        p = CreateSftp(session, data);
                    else
                        p = Build(data, type);
                }
            }
            return p;
        }

        internal static IPacket CreateSftp(Session session, byte[] data)
        {
            IPacket p = null;
            SftpMessageCode code = (SftpMessageCode)data[13];
            if (typeLookupSftp.ContainsKey(code))
            {
                var types = typeLookupSftp[code];
                Type type = types.First();
                if (type != null) p = BuildSftp(data, type);
            }
            return p;
        }

        private static Type SelectorKexCode(Session session, byte[] data)
        {
            var kexAlgorithm = session.Algorithms.KexAlgorithms[0];
            if (kexAlgorithm.StartsWith("ecdh-sha2"))
                return typeof(SshECDHKexReply);
            else if (kexAlgorithm.StartsWith("diffie-hellman-group-exchange"))
                return typeof(SshDHKexReply);
            else if (kexAlgorithm.StartsWith("diffie-hellman-group"))
                return typeof(SshDHKexReplyGroup);
            else
                return null;
        }

        private static Type SelectorSshChannelRequest(Session session, byte[] data)
        {
            var msg = (SshChannelRequest)Build(data, typeof(SshChannelRequest));
            if (msg.RequestType == "exit-status")
                return typeof(SshChannelRequestExitStatus);
            else if (msg.RequestType == "keepalive@openssh.com")
                return typeof(SshChannelRequestKeepAlive);
            else if (msg.RequestType == "exit-signal")
                return typeof(SshChannelRequestExitSignal);
            else
                return typeof(SshChannelRequest);
        }

        private static Type SelectorSshChannelData(Session session, byte[] data)
        {
            using (var pr = new PacketReader(data))
            {
                pr.ReadMessageCode();
                ChannelProcessor.ChannelType channelType;
                var channel = pr.ReadUInt32();

                // Collection can be modified in another thread. It's rare, so just try again.
                try
                {
                    channelType = ((ChannelProcessor)session.Processors.Single(p => p is ChannelProcessor && ((ChannelProcessor)p).LocalChannel == channel)).Type;
                }
                catch (InvalidOperationException)
                {
                    return SelectorSshChannelData(session, data);
                }

                if (channelType == ChannelProcessor.ChannelType.Sftp)
                    return typeof(SftpPacket);
                else
                    return typeof(SshChannelData);
            }
        }

        private static Type SelectorChannelOpen(Session session, byte[] data)
        {
            var msg = (SshChannelOpen)Build(data, typeof(SshChannelOpen));
            if (msg.ChannelType == "forwarded-tcpip")
                return typeof(SshChannelOpenForwardedTcpIp);
            else
                return typeof(SshChannelOpen);
        }
    }
}
