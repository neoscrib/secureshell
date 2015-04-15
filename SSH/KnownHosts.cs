using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SSH.IO;

namespace SSH
{
    public class KnownHosts
    {
        public Func<string, string, byte[], bool> ConfirmUnknownHostDelegate { get; set; }
        public bool ConfirmUnknownHosts { get; set; }
        public string UserKnownHostsFile { get; set; }
        public bool StrictHostKeyChecking { get; set; }

        public KnownHosts()
        {
            ConfirmUnknownHostDelegate = DefaultConfirmUnknownHostDelegate;
            ConfirmUnknownHosts = true;
            StrictHostKeyChecking = true;
        }

        public StatusCode VerifyHostKey(byte[] ks, string hostname, int port)
        {
            if (InternalVerifyHostKey(ks, hostname, port))
            {
                Debug.WriteLine("Host key verified!");
                return StatusCode.OK;
            }
            else
            {
                Debug.WriteLine("Host key couldn't be verified!");
                return StatusCode.HostVerificationFailed;
            }
        }

        private bool InternalVerifyHostKey(byte[] ks, string hostname, int port)
        {
            hostname = FormatHostname(hostname, port);

            var type = string.Empty;
            using (var pr = new PacketReader(ks))
                type = pr.ReadStringAsString();
            var header = string.Format("{0} {1} ", hostname, type);

            string path = GetKnownHostsPath();

            var known = File.ReadAllLines(path);
            var exists = known.Any(s => s.StartsWith(header));
            var verified = false;
            if (exists)
            {
                var stored_key = Convert.FromBase64String(known.Where(s => s.StartsWith(header)).Select(s => s.Split(' ')[2]).Single());
                verified = ks.SequenceEqual(stored_key);
                if (!verified && !StrictHostKeyChecking)
                {
                    Debug.WriteLine("Strict host key checking disabled!!!");
                    verified = true;
                }
            }
            else
            {
                if (ConfirmUnknownHosts)
                {
                    using (var md5 = new MD5CryptoServiceProvider())
                    {
                        verified = (ConfirmUnknownHostDelegate != null &&
                            ConfirmUnknownHostDelegate(hostname, type, md5.ComputeHash(ks)) &&
                            WriteKnownHost(hostname, type, ks, path));
                        if (ConfirmUnknownHostDelegate == null)
                            Debug.WriteLine("No unknown host confirmation delegate defined!!! Can't confirm host key.");
                    }
                }
                else
                {
                    Debug.WriteLine("Not confirming unknown hosts!!!");
                    verified = true;
                }
            }

            return verified;
        }

        private static string FormatHostname(string hostname, int port)
        {
            if (port != 22)
                hostname = string.Format("[{0}]:{1}", hostname, port);
            return hostname;
        }

        private static bool WriteKnownHost(string hostname, string type, byte[] ks, string path)
        {
            var lines = new string[] { string.Format("{0} {1} {2}", hostname, type, Convert.ToBase64String(ks)) };
            File.AppendAllLines(path, lines);
            return true;
        }

        public void RemoveKnownHost(string hostname, int port)
        {
            hostname = string.Format("{0} ", FormatHostname(hostname, port));
            string path = GetKnownHostsPath();
            var known = File.ReadAllLines(path);
            known = known.Where(s => !s.StartsWith(hostname)).ToArray();
            File.WriteAllLines(path, known);
        }

        private string GetKnownHostsPath()
        {
            string path = UserKnownHostsFile;
            if (string.IsNullOrWhiteSpace(path))
            {
                var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                directory = Path.Combine(directory, ".ssh");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory).Attributes |= FileAttributes.Hidden;
                path = Path.Combine(directory, "known_hosts");
                if (!File.Exists(path))
                    File.Create(path);
            }
            return path;
        }

        public static bool DefaultConfirmUnknownHostDelegate(string hostname, string type, byte[] fingerprint)
        {
            string readable = string.Join(":", fingerprint.Select((b) => b.ToString("x").PadLeft(2, '0')));
            var s = new StringBuilder();
            string s1 = "The authenticity of host '{0} ({1})' can't be established. ";
            try
            {
                s.AppendFormat(s1, hostname, Dns.GetHostEntry(hostname).AddressList[0]);
            }
            catch (Exception)
            {
                s.AppendFormat(s1, hostname, hostname);
            }
            type = type == "ssh-rsa" ? "RSA" : type == "ssh-dss" ? "DSA" : type.StartsWith("ecdsa") ? "ECDSA" : string.Empty;
            s.AppendFormat("{0} key fingerprint is {1}. ", type, readable);
            s.Append("Are you sure you want to continue connecting?");
            return MessageBox.Show(s.ToString(), "Host key verification", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes;
        }
    }
}
