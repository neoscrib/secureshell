using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSH.Packets;
using System.Diagnostics;
using System.IO;
using SSH.Identity;

namespace SSH.Processor
{
    class AuthenticationProcessor : PacketProcessor
    {
        private string[] methods = new string[] { "none" };
        private string[] keyfiles = new string[] { };
        private string previous = string.Empty;
        private int currentkey = 0;
        private IIdentityFile identity;

        public AuthenticationProcessor(Session session)
            : base(session)
        {
            DiscoverKeyFiles();
            session.Socket.WritePacket(new SshServiceRequest("ssh-userauth"));
        }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_USERAUTH_FAILURE:
                    var msg = (SshUserAuthFailure)p;
                    SetMethods(msg.Methods.Split(','));
                    Authenticate();
                    return true;
                case MessageCode.SSH_MSG_USERAUTH_BANNER:
                    Debug.WriteLine(((SshUserAuthBanner)p).Message);
                    return true;
                case MessageCode.SSH_MSG_USERAUTH_SUCCESS:
                    Close(SSH.StatusCode.OK);
                    return true;
                case MessageCode.SSH_MSG_USERAUTH_PK_OK:
                    var request = new SshUserAuthRequestSignature(session.Username, identity, session.SessionId);
                    session.Socket.WritePacket(request);
                    return true;
                case MessageCode.SSH_MSG_SERVICE_ACCEPT:
                    Authenticate();
                    return true;
                default:
                    return false;
            }
        }

        private void SetMethods(string[] server)
        {
            this.methods = server;
            if (methods.Length > 0)
            {
                if (currentkey < keyfiles.Length)
                    methods = Guess(methods, new string[] { "publickey", "password" });
                else
                    methods = Guess(methods, new string[] { "password" });

                if ((previous == "password" && methods.Length > 0 && methods[0] == "password") ||
                    (session.Password == null && methods.Length > 0 && methods[0] == "password"))
                    session.Password = session.PasswordFunction();

                if (methods.Length > 0 && methods[0] == "password" && session.Password == null)
                    methods = new string[] { };
            }
        }

        private void Authenticate()
        {
            if (methods.Length > 0)
                Authenticate(previous = methods.First());
            else
                Close(SSH.StatusCode.AuthenticationFailed);
        }

        private void Authenticate(string method)
        {
            IPacket request;

            identity = method == "publickey" && keyfiles != null && keyfiles.Length > 0 && currentkey < keyfiles.Length ?
                IdentityFile.Create(keyfiles[currentkey++]) : null;

            if (identity != null && session.PassphraseFunction != null)
                identity.PassphraseFunction = session.PassphraseFunction;

            if (method == "publickey")
                request = new SshUserAuthRequestPublicKey(session.Username, identity);
            else if (method == "password")
                request = new SshUserAuthRequestPassword(session.Username, session.Password);
            else
                request = new SshUserAuthRequest(session.Username);

            session.Socket.WritePacket(request);
        }

        private string[] Guess(string[] server, string[] client)
        {
            var s = server.ToList();
            var c = client.Where(s1 => s.Contains(s1)).ToList();
            c.Sort((s1, s2) => s.IndexOf(s1).CompareTo(s.IndexOf(s2)));
            return c.ToArray();
        }

        private void DiscoverKeyFiles()
        {
            var files = new List<string>();
            if (!string.IsNullOrWhiteSpace(session.IdentityFile) && File.Exists(session.IdentityFile))
            {
                files.Add(session.IdentityFile);
            }
            else if ((session.Password = session.PasswordFunction()) == null)
            {
                var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                directory = Path.Combine(directory, ".ssh");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory).Attributes |= FileAttributes.Hidden;
                foreach (var f in new string[] { "id_ecdsa", "id_rsa", "id_dsa", "identity" })
                {
                    string path = Path.Combine(directory, f);
                    if (File.Exists(path))
                    {
                        Debug.WriteLine(string.Format("Found private key: {0}", path));
                        files.Add(path);
                    }
                }
            }

            keyfiles = files.ToArray();
        }
    }
}
