using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using SSH.Identity;
using SSH.IO;
using SSH.Packets;
using SSH.Processor;
using System.Net;
using System.Security;

namespace SSH
{
    public class Session : IDisposable
    {
        private int port = 22;

        private Func<SecureString> passwordFunction = () => { return null; };
        internal SecureString Password { get; set; }

        public string Hostname { get; set; }
        public int Port { get { return port; } set { port = value; } }
        public string Username { get; set; }
        public Func<SecureString> PasswordFunction { get { return passwordFunction; } set { passwordFunction = value; } }
        public Func<IIdentityFile, SecureString> PassphraseFunction { get; set; }
        public string IdentityFile { get; set; }
        public bool UseCompression { get; set; }

        public Func<string, string, byte[], bool> ConfirmUnknownHostDelegate { get { return KnownHosts.ConfirmUnknownHostDelegate; } set { KnownHosts.ConfirmUnknownHostDelegate = value; } }
        public bool ConfirmUnknownHosts { get { return KnownHosts.ConfirmUnknownHosts; } set { KnownHosts.ConfirmUnknownHosts = value; } }
        public bool StrictHostKeyChecking { get { return KnownHosts.StrictHostKeyChecking; } set { KnownHosts.StrictHostKeyChecking = value; } }

        internal byte[] SessionId { get; set; }

        internal KeyExchangeProcessor KexProcessor { get; set; }
        internal IPacketProcessor SystemProcessor { get; set; }

        internal SshSocket Socket { get; set; }

        public SshIdentification ServerIdentifier { get; set; }
        public SshIdentification ClientIdentifier { get; set; }
        public SessionProcessor Processors { get; set; }
        public List<IDisposable> Disposables { get; set; }

        public SshKeyExchangeInit Algorithms { get { return KexProcessor.ClientKexInit; } }
        public KnownHosts KnownHosts { get; set; }

        public bool IsAuthenticated { get; set; }

        internal uint NextChannel { get; set; }

        internal StatusCode VerifyHostKey(byte[] ServerHostKey)
        {
            return KnownHosts.VerifyHostKey(ServerHostKey, Hostname, Port);
        }

        public Session()
        {
            Disposables = new List<IDisposable>();
            Processors = new SessionProcessor(this);
            KnownHosts = new KnownHosts();
        }

        public StatusCode Connect()
        {
            SystemProcessor = new SystemProcessor(this);
            KexProcessor = new KeyExchangeProcessor(this);
            var ident = new IdentificationProcessor(this);

            Socket = new SshSocket(this);
            var code = Socket.Connect(Hostname, Port);
            if (code == StatusCode.OK)
            {
                ident.Wait();
                if (ident.StatusCode != StatusCode.OK)
                    return ident.StatusCode;

                KexProcessor.Wait();
                if (KexProcessor.StatusCode != StatusCode.OK)
                    return KexProcessor.StatusCode;

                var auth = new AuthenticationProcessor(this);
                auth.Wait();
                IsAuthenticated = auth.StatusCode == StatusCode.OK;
                return auth.StatusCode;
            }
            return code;
        }

        public void Disconnect(SshDisconnect.SshDisconnectReason reasonCode = SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_BY_APPLICATION, string reason = "")
        {
            for (int i = 0; i < this.Processors.Count; i++)
                this.Processors[i].Close();
            for (int i = 0; i < this.Disposables.Count; i++)
                this.Disposables[i].Dispose();
            this.Socket.WritePacket(new SshDisconnect(reasonCode, reason));
            this.Socket.Disconnect();
        }

        public static string GetStatusDescription(StatusCode code)
        {
            return code.GetType().GetMember(Enum.GetName(code.GetType(), code)).Single().
                GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().Single().Description;
        }

        public static SecureString ConsolePasswordFunction()
        {
            return ConsolePrompt("Password: ", true);
        }

        public static SecureString ConsolePassphraseFunction(IIdentityFile identity)
        {
            return ConsolePrompt("Passphrase: ", true);
        }

        public static SecureString ConsolePrompt(string prompt, bool mask = false)
        {
            Console.Write(prompt);
            var x = new SecureString();
            var key = Console.ReadKey(mask);
            while (key.Key != ConsoleKey.Enter)
            {
                x.AppendChar(key.KeyChar);
                key = Console.ReadKey(mask);
            }
            Console.WriteLine();
            return x;
        }

        public Shell CreateShell(bool useConsole = false, bool removeTerminalEmulationCharacters = true)
        {
            return new Shell(this, useConsole, removeTerminalEmulationCharacters);
        }

        public Exec CreateExec()
        {
            return new Exec(this);
        }

        public Sftp CreateSftp()
        {
            return new Sftp(this);
        }

        public DirectTcpIp CreateLocalForward(uint localPort, string remoteAddress, uint remotePort)
        {
            return new DirectTcpIp(this, new IPEndPoint(IPAddress.Any, (int)localPort), remoteAddress, remotePort);
        }

        public RemoteForward CreateRemoteForward(string localAddress, uint localPort, string remoteAddress, uint remotePort)
        {
            return new RemoteForward(this, localAddress, localPort, remoteAddress, remotePort);
        }

        public DynamicSocks CreateSocks(IPEndPoint localEndpoint)
        {
            return new DynamicSocks(this, localEndpoint);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Disconnect();
        }
    }
}
