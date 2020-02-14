# SSH Client Library for .Net

A client library utilizing the Secure Shell protocol for connecting to remote systems over a secure connection with the .Net Framework. Includes support for tty, sftp, local port forwarding, remote port forwarding, dynamic port forwarding, and remote execution. Additionally supports the following:

Key Exchange: Diffie Hellman Group 1 and Group 14 with SHA1, and Group Exhange with SHA1 or SHA256, Elliptic Curve Diffie Hellman with SHA2 and NIST curves 256, 384, and 521

Encryption: AES128, 192, and 256, and TripleDES in Counter or CBC mode, Cast128 and Blowfish in CBC mode, and TripleDES in EBC mode

Authentication: Password, PublicKey (OpenSSH DSS, RSA, and ECDSA, PuTTy DSS, and RSA)

Message Authentication: HMAC using SHA256, 384, and 512, SHA1, RipeMD160, MD5, MD5-96, and SHA1-96

### Example TTY Usage 
```c#
using SSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SshTestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Hostname: ");
            var hostname = Console.ReadLine();
            Console.Write("Username: ");
            var username = Console.ReadLine();

            using (var ssh = new Session())
            {
                ssh.Hostname = hostname;
                ssh.Username = username;
                ssh.PasswordFunction = Session.ConsolePasswordFunction;
                ssh.UseCompression = true;

                if (ssh.Connect() == StatusCode.OK)
                {
                    using (var shell = ssh.CreateShell(true))
                    {
                        shell.Wait();
                    }
                }
            }
        }
    }
}

```