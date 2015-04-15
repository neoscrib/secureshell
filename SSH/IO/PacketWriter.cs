using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SSH.Packets;

namespace SSH.IO
{
    public class PacketWriter : BinaryWriter
    {
        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private byte[] buffer = new byte[16];

        public PacketWriter(Stream s) : base(s) { }

        public PacketWriter() : base(new MemoryStream()) { }

        public void Write(Enum messageCode)
        {
            base.Write(Convert.ToByte(messageCode));
        }

        public void WriteInt16(short value)
        {
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
            BaseStream.Write(buffer, 0, 2);
        }

        public void WriteInt32(int value)
        {
            buffer[0] = (byte)(value >> 24);
            buffer[1] = (byte)(value >> 16);
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)value;
            BaseStream.Write(buffer, 0, 4);
        }

        public void WriteUInt32(uint value)
        {
            buffer[0] = (byte)(value >> 24);
            buffer[1] = (byte)(value >> 16);
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)value;
            BaseStream.Write(buffer, 0, 4);
        }

        public void WriteInt64(long value)
        {
            buffer[0] = (byte)(value >> 56);
            buffer[1] = (byte)(value >> 48);
            buffer[2] = (byte)(value >> 40);
            buffer[3] = (byte)(value >> 32);
            buffer[4] = (byte)(value >> 24);
            buffer[5] = (byte)(value >> 16);
            buffer[6] = (byte)(value >> 8);
            buffer[7] = (byte)value;
            BaseStream.Write(buffer, 0, 8);
        }

        public void WriteUInt64(ulong value)
        {
            buffer[0] = (byte)(value >> 56);
            buffer[1] = (byte)(value >> 48);
            buffer[2] = (byte)(value >> 40);
            buffer[3] = (byte)(value >> 32);
            buffer[4] = (byte)(value >> 24);
            buffer[5] = (byte)(value >> 16);
            buffer[6] = (byte)(value >> 8);
            buffer[7] = (byte)value;
            BaseStream.Write(buffer, 0, 8);
        }

        public void WriteString(byte[] buffer)
        {
            WriteInt32(buffer.Length);
            BaseStream.Write(buffer, 0, buffer.Length);
        }

        public void WriteBytes(string s)
        {
            WriteString(System.Text.Encoding.Default.GetBytes(s));
        }

        public void WriteStringAsString(string s)
        {
            base.Write(System.Text.Encoding.Default.GetBytes(s));
        }

        public override void Write(byte[] buffer)
        {
            BaseStream.Write(buffer, 0, buffer.Length);
        }

        public void WriteLine(string s)
        {
            WriteStringAsString(s + "\r\n");
        }

        public void WriteRandom(int numbytes)
        {
            var bytes = new byte[numbytes];
            rng.GetBytes(bytes);
            Write(bytes);
        }

        public void WriteProperty(SshPropertyInfo p, IPacket packet)
        {
            Type type = p.Information.PropertyType;
            object value = p.Information.GetValue(packet, null);

            // ordered by most common type.
            if (type.Equals(typeof(uint)))
                this.WriteUInt32((uint)value);
            else if (typeof(Enum).IsAssignableFrom(type) && Enum.GetUnderlyingType(type).Equals(typeof(byte)))
                this.Write((byte)value);
            else if (type.Equals(typeof(string[])))
                this.WriteBytes(((string[])value).ToCSV());
            else if (type.Equals(typeof(string)))
                this.WriteBytes((string)value);
            else if (type.Equals(typeof(byte[])))
            {
                if (p.Attributes.Raw)
                    this.Write((byte[])value);
                else
                    this.WriteString((byte[])value);
            }
            else if (type.Equals(typeof(bool)))
                this.Write((bool)value);
            else if (typeof(Enum).IsAssignableFrom(type) && Enum.GetUnderlyingType(type).Equals(typeof(uint)))
                this.WriteUInt32((uint)value);
            else if (type.Equals(typeof(short)))
                this.WriteInt16((short)value);
            else if (type.Equals(typeof(int)))
                this.WriteInt32((int)value);
            else if (type.Equals(typeof(long)))
                this.WriteInt64((long)value);
            else if (type.Equals(typeof(ulong)))
                this.WriteUInt64((ulong)value);
            else if (type.Equals(typeof(Dictionary<string, string>)))
                this.WriteDictionary((Dictionary<string, string>)value);
            else if (type.Equals(typeof(SftpFileAttributes)))
                this.WriteSftpFileAttributes((SftpFileAttributes)value);
        }

        private void WriteDictionary(Dictionary<string, string> d)
        {
            foreach (var i in d)
            {
                this.WriteBytes(i.Key);
                this.WriteBytes(i.Value);
            }
        }

        private void WriteSftpFileAttributes(SftpFileAttributes attrs)
        {
            this.WriteUInt32((uint)attrs.Flags);
            if (attrs.Flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_PERMISSIONS))
                this.WriteUInt32((uint)attrs.Permissions);
        }
    }
}
