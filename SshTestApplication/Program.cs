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
