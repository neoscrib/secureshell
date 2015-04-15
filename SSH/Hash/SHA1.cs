using System.Security.Cryptography;
using SSH.IO;

namespace SSH.Hash
{
    [HashWriter("diffie-hellman-group-exchange-sha1")]
    [HashWriter("diffie-hellman-group14-sha1")]
    [HashWriter("diffie-hellman-group1-sha1")]
    [HashWriter("ssh-rsa")]
    [HashWriter("ssh-dss")]
    class SHA1 : HashWriter
    {
        public SHA1()
            : base(new SHA1Managed())
        {
            name = "SHA1";
        }
    }
}
