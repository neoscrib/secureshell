using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Hash
{
    [HashWriter("ecdh-sha2-nistp256")]
    [HashWriter("ecdsa-sha2-nistp256")]
    [HashWriter("curve25519-sha256@libssh.org")]
    [HashWriter("diffie-hellman-group-exchange-sha256")]
    class SHA256 : HashWriter
    {
        public SHA256()
            : base(new SHA256Managed())
        {
            name = "SHA256";
        }
    }
}
