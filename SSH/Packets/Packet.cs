using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SSH.IO;

namespace SSH.Packets
{
    public class Packet : IPacket
    {
        [SshProperty(0)]
        public MessageCode Code { get; set; }

        public bool Handled { get; set; }

        private static Dictionary<Type, SshPropertyInfo[]> properties { get; set; }
        private static EventWaitHandle getPropertiesWaitHandle;

        static Packet()
        {
            properties = new Dictionary<Type, SshPropertyInfo[]>();
            getPropertiesWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
        }

        public Packet(MessageCode code)
        {
            this.Code = code;
        }

        public override string ToString()
        {
            var s = new StringBuilder(string.Format("{0}\n", this.GetType().Name));
            var props = Packet.GetProperties(this.GetType());

            foreach (var p in props)
            {
                Type type = p.Information.PropertyType;
                string name = p.Information.Name;
                object value = p.Information.GetValue(this, null);

                s.AppendFormat("  {0} : {1}\n", name,
                    value == null ? "null" :
                    type.Equals(typeof(string[])) ? string.Join(", ", (string[])value) :
                    type.Equals(typeof(byte[])) ? string.Format("Length: {0}", ((byte[])value).Length) + ((byte[])value).HexDump() : 
                    type.Equals(typeof(Dictionary<string, string>)) ? string.Format("\n    {0}", string.Join("\n    ", ((Dictionary<string, string>)value).Select(kvp => string.Format("{0} : {1}", kvp.Key, kvp.Value)))) :
                    value is Enum ? string.Format("{0} ({1})", value, Convert.ToInt32(value)) :
                    value);
            }
            return s.ToString();
        }

        public byte[] ToSshMessage()
        {
            var props = Packet.GetProperties(this.GetType());

            if (this is SshIdentification)
                return ((SshIdentification)this).Identifier.ToByteArray();

            using (var pw = new PacketWriter())
            {
                foreach (var p in props)
                    pw.WriteProperty(p, this);
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }

        internal static SshPropertyInfo[] GetProperties(Type type)
        {
            getPropertiesWaitHandle.WaitOne();
            if (!properties.ContainsKey(type))
            {
                var props = type.GetProperties().
                    Where(p => p.GetCustomAttributes(typeof(SshPropertyAttribute), true).Count() > 0).
                    Select(p => new SshPropertyInfo(
                        (SshPropertyAttribute)p.GetCustomAttributes(typeof(SshPropertyAttribute), true).Single(), p)).
                    OrderBy(p => p.Attributes.Order);
                properties.Add(type, props.ToArray());
            }
            getPropertiesWaitHandle.Set();
            return properties[type];
        }
    }
}
