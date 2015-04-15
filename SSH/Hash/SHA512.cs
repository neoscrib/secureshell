using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Hash
{
    [HashWriter("ecdh-sha2-nistp521")]
    [HashWriter("ecdsa-sha2-nistp521")]
    class SHA512 : HashWriter
    {
        public SHA512() :
            base(new SHA512Managed())
        {
            name = "SHA512";
        }
    }
}
