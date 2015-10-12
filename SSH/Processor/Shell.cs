using SSH.IO;
using SSH.Packets;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SSH.Processor
{
    public class Shell : ChannelProcessor
    {
        public bool UseConsole { get; set; }
        public bool RemoveTerminalEmulationCharacters { get; set; }
        public Stream InputStream { get; private set; }
        public Stream OutputStream { get; set; }

        private bool tty_opened = false;

        private Thread readConsoleThread;
        private EventWaitHandle waitHandle;
        private bool closeSent = false;

        public event EventHandler DataReceived
        {
            add { if (OutputStream is NotifyStream) ((NotifyStream)OutputStream).DataReceived += value; }
            remove { if (OutputStream is NotifyStream) ((NotifyStream)OutputStream).DataReceived -= value; }
        }

        public Shell(Session session, bool useConsole = false, bool removeTerminalEmulationCharacters = true)
            : base(session)
        {
            Type = ChannelType.Shell;
            RemoveTerminalEmulationCharacters = removeTerminalEmulationCharacters;
            UseConsole = useConsole;

            if (!UseConsole)
            {
                InputStream = new NotifyStream();
                ((NotifyStream)InputStream).DataReceived += new EventHandler(Shell_DataReceived);
                OutputStream = new NotifyStream();
            }
            else
            {
                Console.Clear();
                Console.TreatControlCAsInput = true;
            }

            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            session.Socket.WritePacket(new SshChannelOpen("session", this.LocalChannel, 0xffffffff, 0x4000));
            waitHandle.WaitOne();
        }

        public override void Close()
        {
            closeSent = true;
            session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
            waitHandle.WaitOne();
            Close(StatusCode.OK);
        }

        private void WriteConsole(byte[] data)
        {
            if (RemoveTerminalEmulationCharacters)
            {
                var data2 = HandleTerminalChars(data);
                Console.Write(System.Text.Encoding.UTF8.GetChars(data2));
            }
            else
            {
                Console.Write(System.Text.Encoding.UTF8.GetChars(data));
            }
        }

        void ReadConsole()
        {
            try
            {
                while (UseConsole && Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Aborted)
                {
                    var key = Console.ReadKey(true);
                    byte[] b = new byte[] { (byte)key.KeyChar };
                    session.Socket.WritePacket(new SshChannelData(this.RemoteChannel, b));
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("ShellChannelProcessor thread aborted.");
                Thread.ResetAbort();
            }
            finally
            {
                session.Socket.WritePacket(new SshChannelEOF(this.RemoteChannel));
            }
        }

        void Shell_DataReceived(object sender, EventArgs e)
        {
            var data = new byte[InputStream.Length - InputStream.Position];
            var length = InputStream.Read(data, 0, data.Length);
            if (length != data.Length)
            {
                var copy = new byte[length];
                Buffer.BlockCopy(data, 0, copy, 0, length);
                data = copy;
            }
            session.Socket.WritePacket(new SshChannelData(this.RemoteChannel, data));
        }

        private static string escapeCharsPattern = @"\e\[[0-9;?]*[^0-9;]";
        public static string HandleTerminalChars(string str)
        {
            str = str.Replace("(B)0", "");
            str = Regex.Replace(str, escapeCharsPattern, "");
            str = str.Replace(((char)15).ToString(), "");
            str = Regex.Replace(str, ((char)27) + "=*", "");
            //str = Regex.Replace(str, "\\s*\r\n", "\r\n");
            return str;
        }

        public static byte[] HandleTerminalChars(byte[] str)
        {
            var s = System.Text.Encoding.Default.GetString(str);
            return HandleTerminalChars(s).ToByteArray();
        }

        public override void OnChannelOpenConfirmation(ISshChannelMessage p)
        {
            var msg = (SshChannelOpenConfirmation)p;
            this.RemoteChannel = msg.SenderChannel;
            session.Socket.WritePacket(new SshChannelRequestPty(this.RemoteChannel, "dumb", 80, 25, 640, 480, string.Empty));
        }

        public override void OnChannelOpenFailure(ISshChannelMessage p) { }

        public override void OnChannelSuccess(ISshChannelMessage p)
        {
            if (!tty_opened)
            {
                tty_opened = true;
                session.Socket.WritePacket(new SshChannelRequest(this.RemoteChannel, "shell", true));
            }
            else
            {
                if (UseConsole)
                {
                    readConsoleThread = new Thread(new ThreadStart(ReadConsole));
                    readConsoleThread.Start();
                }
                waitHandle.Set();
            }
        }

        public override void OnChannelWindowAdjust(ISshChannelMessage p) { }

        public override void OnChannelData(ISshChannelMessage p)
        {
            var msg = (SshChannelData)p;
            if (UseConsole)
                WriteConsole(msg.Data);
            else
                OutputStream.Write(msg.Data, 0, msg.Data.Length);
        }

        public override void OnChannelExtendedData(ISshChannelMessage p) { }

        public override void OnChannelClose(ISshChannelMessage p)
        {
            OnChannelEndOfFile(p);
            if (!closeSent)
            {
                closeSent = true;
                session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
                Close(StatusCode.OK);
            }
            waitHandle.Set();
        }

        public override void OnChannelEndOfFile(ISshChannelMessage p)
        {
            if (UseConsole && readConsoleThread != null && readConsoleThread.IsAlive)
            {
                readConsoleThread.Abort();
                readConsoleThread.Join();
            }
        }

        public override void OnChannelRequest(ISshChannelMessage p) { }
    }
}
