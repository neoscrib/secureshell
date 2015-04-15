using System;
using System.Collections.Generic;
using System.IO;
using SSH.Packets;

namespace SSH.IO
{
    public class PacketReader : BinaryReader
    {
        private byte[] buffer = new byte[16];

        public long Position { get { return BaseStream.Position; } set { BaseStream.Position = value; } }
        public long Length { get { return BaseStream.Length; } }

        public PacketReader(Stream s) : base(s) { }

        public PacketReader(byte[] data) : base(new MemoryStream(data)) { }

        protected override void FillBuffer(int numBytes)
        {
            BaseStream.Read(buffer, 0, numBytes);
        }

        public MessageCode ReadMessageCode()
        {
            return (MessageCode)base.ReadByte();
        }

        public override short ReadInt16()
        {
            FillBuffer(2);
            return (short)((int)buffer[0] << 8 | (int)buffer[1]);
        }

        public override int ReadInt32()
        {
            FillBuffer(4);
            return (int)buffer[0] << 24 | (int)buffer[1] << 16 | (int)buffer[2] << 8 | (int)buffer[3];
        }

        public override uint ReadUInt32()
        {
            FillBuffer(4);
            return (uint)((int)buffer[0] << 24 | (int)buffer[1] << 16 | (int)buffer[2] << 8 | (int)buffer[3]);
        }

        public override long ReadInt64()
        {
            FillBuffer(8);
            uint num1 = (uint)((int)buffer[0] << 24 | (int)buffer[1] << 16 | (int)buffer[2] << 8 | (int)buffer[3]);
            uint num2 = (uint)((int)buffer[4] << 24 | (int)buffer[5] << 16 | (int)buffer[6] << 8 | (int)buffer[7]);
            return (long)((ulong)num1 << 32 | (ulong)num2);
        }

        public override ulong ReadUInt64()
        {
            FillBuffer(8);
            uint num1 = (uint)((int)buffer[0] << 24 | (int)buffer[1] << 16 | (int)buffer[2] << 8 | (int)buffer[3]);
            uint num2 = (uint)((int)buffer[4] << 24 | (int)buffer[5] << 16 | (int)buffer[6] << 8 | (int)buffer[7]);
            return (ulong)num1 << 32 | (ulong)num2;
        }

        public new byte[] ReadString()
        {
            int length = ReadInt32();
            return ReadBytes(length);
        }

        public string ReadStringAsString()
        {
            return System.Text.Encoding.Default.GetString(ReadString());
        }

        public string ReadLine()
        {
            string s = string.Empty;
            char c;
            while ((c = base.ReadChar()) != '\r' && c != '\n')
                s += c;
            if (c == '\r')
                base.ReadChar();
            return s;
        }

        public object ReadProperty(SshPropertyInfo p)
        {
            Type type = p.Information.PropertyType;

            // ordered by most common type.
            if (typeof(Enum).IsAssignableFrom(type) && Enum.GetUnderlyingType(type).Equals(typeof(byte)))
                return Enum.ToObject(type, this.ReadByte());
            else if (type.Equals(typeof(string[])))
                return this.ReadStringAsString().Split(',');
            else if (type.Equals(typeof(string)))
                return this.ReadStringAsString();
            else if (type.Equals(typeof(uint)))
                return this.ReadUInt32();
            else if (type.Equals(typeof(byte[])))
            {
                if (p.Attributes.Raw)
                    return this.ReadBytes(p.Attributes.RawLength);
                else
                    return this.ReadString();
            }
            else if (type.Equals(typeof(bool)))
                return this.ReadBoolean();
            else if (typeof(Enum).IsAssignableFrom(type) && Enum.GetUnderlyingType(type).Equals(typeof(uint)))
                return Enum.ToObject(type, this.ReadUInt32());
            else if (type.Equals(typeof(short)))
                return this.ReadInt16();
            else if (type.Equals(typeof(int)))
                return this.ReadInt32();
            else if (type.Equals(typeof(long)))
                return this.ReadInt64();
            else if (type.Equals(typeof(ulong)))
                return this.ReadUInt64();
            else if (type.Equals(typeof(Dictionary<string, string>)))
            {
                var d = new Dictionary<string, string>();
                while (this.Position < this.Length)
                {
                    d.Add(this.ReadStringAsString(), this.ReadStringAsString());
                }
                return d;
            }
            else if (type.Equals(typeof(SftpFileAttributes)))
            {
                var v = new SftpFileAttributes();
                v.Flags = (FileInfoFlags)this.ReadUInt32();
                if (v.Flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_PERMISSIONS))
                    v.Permissions = (PermissionsFlags)this.ReadUInt32();
            }
            else if (type.Equals(typeof(SftpFileInfo)))
            {
                return new SftpFileInfo(this, string.Empty, string.Empty, string.Empty);
            }
            else if (type.Equals(typeof(SftpFileInfo[])))
            {
                var d = new List<SftpFileInfo>();
                while (this.Position < this.Length)
                {
                    d.Add(new SftpFileInfo(this, string.Empty));
                }
                return d.ToArray();
            }

            return null;
        }
    }
}
