using SSH.IO;
using SSH.Packets;
using System;
using System.IO;
using System.Threading;

namespace SSH.Processor
{
    public class Exec : ChannelProcessor
    {
        public string Command { get; set; }
        public Stream StandardOut { get; set; }
        public Stream StandardError { get; set; }

        public event EventHandler StandardOutDataReceived
        {
            add { if (StandardOut is NotifyStream) ((NotifyStream)StandardOut).DataReceived += value; }
            remove { if (StandardOut is NotifyStream) ((NotifyStream)StandardOut).DataReceived -= value; }
        }
        public event EventHandler StandardErrorDataReceived
        {
            add { if (StandardError is NotifyStream) ((NotifyStream)StandardError).DataReceived += value; }
            remove { if (StandardError is NotifyStream) ((NotifyStream)StandardError).DataReceived -= value; }
        }
        private EventWaitHandle waitHandle;

        public Exec(Session session)
            : base(session)
        {
            Type = ChannelType.Exec;
            StandardOut = new NotifyStream();
            StandardError = new NotifyStream();
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public override void Close()
        {
            Close(SSH.StatusCode.OK);
        }

        public uint RunCommand(string command)
        {
            session.Socket.WritePacket(new SshChannelOpen("session", this.LocalChannel, 0xffffffff, 0x4000));
            waitHandle.WaitOne();
            session.Socket.WritePacket(new SshChannelRequestExec(this.RemoteChannel, command));
            waitHandle.WaitOne();
            return ExitCode;
        }

        public override void OnChannelOpenConfirmation(ISshChannelMessage p)
        {
            var msg = (SshChannelOpenConfirmation)p;
            this.RemoteChannel = msg.SenderChannel;
            waitHandle.Set();
        }

        public override void OnChannelOpenFailure(ISshChannelMessage p) { }

        public override void OnChannelSuccess(ISshChannelMessage p) { }

        public override void OnChannelWindowAdjust(ISshChannelMessage p) { }

        public override void OnChannelData(ISshChannelMessage p)
        {
            var msg = (SshChannelData)p;
            StandardOut.Write(msg.Data, 0, msg.Data.Length);
        }

        public override void OnChannelExtendedData(ISshChannelMessage p)
        {
            var msg = (SshChannelExtendedData)p;
            StandardError.Write(msg.Data, 0, msg.Data.Length);
        }

        public override void OnChannelClose(ISshChannelMessage p)
        {
            session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
            waitHandle.Set();
        }

        public override void OnChannelEndOfFile(ISshChannelMessage p) { }

        public override void OnChannelRequest(ISshChannelMessage p) { }
    }
}
