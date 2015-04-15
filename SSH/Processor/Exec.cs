using System;
using System.IO;
using System.Threading;
using SSH.IO;
using SSH.Packets;
using System.Diagnostics;

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

        public override bool InternalProcessPacket(ISshChannelMessage p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_CHANNEL_OPEN_CONFIRMATION:
                    {
                        var msg = (SshChannelOpenConfirmation)p;
                        this.RemoteChannel = msg.SenderChannel;
                        waitHandle.Set();
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_SUCCESS:
                    return true;
                case MessageCode.SSH_MSG_CHANNEL_WINDOW_ADJUST:
                    return true;
                case MessageCode.SSH_MSG_CHANNEL_DATA:
                    {
                        var msg = (SshChannelData)p;
                        StandardOut.Write(msg.Data, 0, msg.Data.Length);
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_EXTENDED_DATA:
                    {
                        var msg = (SshChannelExtendedData)p;
                        StandardError.Write(msg.Data, 0, msg.Data.Length);
                        return true;
                    }
                case MessageCode.SSH_MSG_CHANNEL_CLOSE:
                    session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
                    waitHandle.Set();
                    return true;
                case MessageCode.SSH_MSG_CHANNEL_EOF:
                    return true;
                case MessageCode.SSH_MSG_CHANNEL_REQUEST:
                    return false;
            }
            return false;
        }
    }
}
