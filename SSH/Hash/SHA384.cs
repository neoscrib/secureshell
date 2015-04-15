using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Hash
{
    [HashWriter("ecdh-sha2-nistp384")]
    [HashWriter("ecdsa-sha2-nistp384")]
    class SHA384 : HashWriter
    {
        public SHA384() :
            base(new SHA384Managed())
        {
            name = "SHA384";
        }
    }
}
