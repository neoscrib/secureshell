using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Packets
{
    class SftpFileAttributes
    {
        public FileInfoFlags Flags { get; set; }
        public PermissionsFlags Permissions { get; set; }

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

        public static SftpFileAttributes ParsePermissions(string permissions)
        {
            var attrs = new SftpFileAttributes();
            attrs.Flags = string.IsNullOrWhiteSpace(permissions) ? FileInfoFlags.SSH_FILEXFER_ATTR_NONE : FileInfoFlags.SSH_FILEXFER_ATTR_PERMISSIONS;
            attrs.Permissions = (PermissionsFlags)Convert.ToUInt32(string.IsNullOrWhiteSpace(permissions) ? "0" : permissions, 8);
            return attrs;
        }
    }
}
