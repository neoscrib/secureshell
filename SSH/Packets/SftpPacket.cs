using System.IO;
using System.Linq;
using SSH.IO;

namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_DATA)]
    public class SftpPacket : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public SftpMessageCode SftpCode { get; set; }

        public SftpPacket() : base(MessageCode.SSH_MSG_CHANNEL_DATA) { }

        public SftpPacket(SftpMessageCode code)
            : this()
        {
            this.SftpCode = code;
        }

        public SftpPacket(uint channel, SftpMessageCode code) :
            this(code)
        {
            this.Channel = channel;
        }

        public new byte[] ToSshMessage()
        {
            var props = Packet.GetProperties(this.GetType()).Where(p => p.Attributes.Order > 1);
            byte[] sftpData;

            using (var pw = new PacketWriter())
            {
                foreach (var p in props)
                    pw.WriteProperty(p, this);
                using (var pw1 = new PacketWriter())
                {
                    pw1.WriteString(((MemoryStream)pw.BaseStream).ToArray());
                    sftpData = ((MemoryStream)pw1.BaseStream).ToArray();
                }
            }

            props = Packet.GetProperties(this.GetType()).Where(p => p.Attributes.Order <= 1);
            using (var pw = new PacketWriter())
            {
                foreach (var p in props)
                    pw.WriteProperty(p, this);
                pw.WriteString(sftpData);
                return ((MemoryStream)pw.BaseStream).ToArray();
            }
        }
    }
}
