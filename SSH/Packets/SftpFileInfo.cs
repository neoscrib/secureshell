using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SSH.IO;

namespace SSH.Packets
{
    public class SftpFileInfo
    {
        public string Name { get; set; }
        public string Extension { get { return Path.GetExtension(FullName); } }
        public string FullName { get { return Path.Combine(DirectoryName, Name).Replace('\\', '/'); } }
        public string DirectoryName { get; set; }
        public ulong Length { get; set; }
        public PermissionsFlags Permissions { get; set; }
        public DateTime LastAccessTime { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public bool IsDirectory { get { return Permissions.HasFlag(PermissionsFlags.POSIX_PERMS_IFDIR); } }
        public bool IsRegularFile { get { return Permissions.HasFlag(PermissionsFlags.POSIX_PERMS_IFREG); } }
        public bool IsSymbolicLink { get { return Permissions.HasFlag(PermissionsFlags.POSIX_PERMS_IFLNK); } }

        public SftpFileInfo(string name, string directoryName, ulong length, PermissionsFlags permissions, DateTime lastAccessTime, DateTime lastWriteTime)
        {
            this.Name = name;
            this.DirectoryName = directoryName;
            this.Length = length;
            this.Permissions = permissions;
            this.LastAccessTime = lastAccessTime;
            this.LastWriteTime = lastWriteTime;
        }

        public SftpFileInfo(PacketReader pr, string basepath)
            : this(pr, basepath, pr.ReadStringAsString(), pr.ReadStringAsString())
        { }

        public SftpFileInfo(PacketReader pr, string basepath, string filename, string longname)
        {
            DirectoryName = basepath;
            Name = filename;

            FileInfoFlags flags = (FileInfoFlags)pr.ReadUInt32();
            if (flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_SIZE))
            {
                Length = pr.ReadUInt64();
            }
            if (flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_UIDGID))
            {
                pr.ReadUInt32(); pr.ReadUInt32(); // user, group
            }
            if (flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_PERMISSIONS))
            {
                Permissions = (PermissionsFlags)pr.ReadUInt32();
            }
            if (flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_ACMODTIME))
            {
                LastAccessTime = Util.ConvertEpoch((double)pr.ReadUInt32() * 1000);
                LastWriteTime = Util.ConvertEpoch((double)pr.ReadUInt32() * 1000);
            }
            if (flags.HasFlag(FileInfoFlags.SSH_FILEXFER_ATTR_EXTENDED))
            {
                int extendedCount = pr.ReadInt32();
                for (int j = 0; j < extendedCount; j++)
                {
                    pr.ReadString(); pr.ReadString(); // extended type, extended data
                }
            }
        }

        public override string ToString()
        {
            var s = new StringBuilder(string.Format("{0}\n", this.GetType().Name));
            var props = this.GetType().GetProperties();

            foreach (var p in props)
            {
                Type type = p.PropertyType;
                string name = p.Name;
                object value = p.GetValue(this, null);

                s.AppendFormat("    {0} : {1}\n", name,
                    value == null ? "null" :
                    type.Equals(typeof(string[])) ? string.Join(", ", (string[])value) :
                    type.Equals(typeof(byte[])) ? string.Format("Length: {0}", ((byte[])value).Length) + ((byte[])value).HexDump() :
                    value);
            }
            return s.ToString();
        }
    }

    [Flags]
    public enum FileInfoFlags : uint
    {
        SSH_FILEXFER_ATTR_NONE = 0x00,
        SSH_FILEXFER_ATTR_SIZE = 0x01,
        SSH_FILEXFER_ATTR_UIDGID = 0x02,
        SSH_FILEXFER_ATTR_PERMISSIONS = 0x04,
        SSH_FILEXFER_ATTR_ACMODTIME = 0x08,
        SSH_FILEXFER_ATTR_EXTENDED = 0x80000000
    }

    [Flags]
    public enum PermissionsFlags : uint
    {
        POSIX_PERMS_OTHER_EXECUTE = 0x0001,
        POSIX_PERMS_OTHER_WRITE = 0x0002,
        POSIX_PERMS_OTHER_READ = 0x0004,
        POSIX_PERMS_GROUP_EXECUTE = 0x0008,
        POSIX_PERMS_GROUP_WRITE = 0x0010,
        POSIX_PERMS_GROUP_READ = 0x0020,
        POSIX_PERMS_OWNER_EXECUTE = 0x0040,
        POSIX_PERMS_OWNER_WRITE = 0x0080,
        POSIX_PERMS_OWNER_READ = 0x0100,
        POSIX_PERMS_IFDIR = 0x4000,
        POSIX_PERMS_IFREG = 0x8000,
        POSIX_PERMS_IFLNK = 0xA000
    }
}
