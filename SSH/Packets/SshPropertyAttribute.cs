using System;
using System.Reflection;

namespace SSH.Packets
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SshPropertyAttribute : Attribute
    {
        public byte Order { get; set; }
        public bool Raw { get; set; }
        public int RawLength { get; set; }

        public SshPropertyAttribute(byte order)
        {
            this.Order = order;
        }
    }

    public class SshPropertyInfo
    {
        public SshPropertyAttribute Attributes { get; set; }
        public PropertyInfo Information { get; set; }

        public SshPropertyInfo(SshPropertyAttribute attrs, PropertyInfo pi)
        {
            this.Attributes = attrs;
            this.Information = pi;
        }
    }
}
