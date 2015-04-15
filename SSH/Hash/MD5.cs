using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Hash
{
    class MD5 : HashWriter
    {
        public MD5()
            : base(new MD5CryptoServiceProvider())
        {
            name = "MD5";
        }
    }
}
