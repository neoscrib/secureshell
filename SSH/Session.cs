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

namespace SSH
{
    public class Session : IDisposable
    {
        private int port = 22;

        private Func<string> passwordFunction = () => { return null; };
        internal string Password { get; set; }

        public string Hostname { get; set; }
        public int Port { get { return port; } set { port = value; } }
        public string Username { get; set; }
        public Func<string> PasswordFunction { get { return passwordFunction; } set { passwordFunction = value; } }
        public Func<IIdentityFile, string> PassphraseFunction { get; set; }
        public string IdentityFile { get; set; }

        public Func<string, string, byte[], bool> ConfirmUnknownHostDelegate { get { return KnownHosts.ConfirmUnknownHostDelegate; } set { KnownHosts.ConfirmUnknownHostDelegate = value; } }
        public bool ConfirmUnknownHosts { get { return KnownHosts.ConfirmUnknownHosts; } set { KnownHosts.ConfirmUnknownHosts = value; } }
        public bool StrictHostKeyChecking { get { return KnownHosts.StrictHostKeyChecking; } set { KnownHosts.StrictHostKeyChecking = value; } }

        internal byte[] SessionId { get; set; }
        internal byte[][] Keys { get; set; }

        internal KeyExchangeProcessor KexProcessor { get; set; }
        internal IPacketProcessor SystemProcessor { get; set; }

        internal SshSocket Socket { get; set; }

        public SshIdentification ServerIdentifier { get; set; }
        public SshIdentification ClientIdentifier { get; set; }
        public SessionProcessor Processors { get; set; }
        public List<IDisposable> Disposables { get; set; }

        public SshKeyExchangeInit Algorithms { get { return KexProcessor.ClientKexInit; } }
        public KnownHosts KnownHosts { get; set; }

        internal uint NextChannel { get; set; }

        internal StatusCode VerifyHostKey(byte[] ServerHostKey)
        {
            return KnownHosts.VerifyHostKey(ServerHostKey, Hostname, Port);
        }

        public Session()
        {
            Disposables = new List<IDisposable>();
            Keys = new byte[6][];
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
                return auth.StatusCode;
            }
            return code;
        }

        public void Disconnect(SshDisconnect.SshDisconnectReason reasonCode = SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_BY_APPLICATION, string reason = "")
        {
            for (int i = 0; i < this.Processors.Count; )
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

        public static string ConsolePasswordFunction()
        {
            return ConsolePrompt("Password: ", true);
        }

        public static string ConsolePassphraseFunction(IIdentityFile identity)
        {
            return ConsolePrompt("Passphrase: ", true);
        }

        public static string ConsolePrompt(string prompt, bool mask = false)
        {
            Console.Write(prompt);
            string p = string.Empty;
            var key = Console.ReadKey(mask);
            while (key.Key != ConsoleKey.Enter)
            {
                p += key.KeyChar;
                key = Console.ReadKey(true);
            }
            Console.WriteLine();
            return p;
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
            Disconnect();
        }
    }
}
