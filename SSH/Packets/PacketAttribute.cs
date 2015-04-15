using System;

namespace SSH.Packets
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PacketAttribute : Attribute
    {
        public MessageCode MessageCode { get; set; }

        public PacketAttribute(MessageCode code)
        {
            MessageCode = code;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SftpPacketAttribute : Attribute
    {
        public SftpMessageCode MessageCode { get; set; }

        public SftpPacketAttribute(SftpMessageCode code)
        {
            this.MessageCode = code;
        }
    }
}
