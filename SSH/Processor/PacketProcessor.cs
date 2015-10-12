using System.Threading;
using SSH.Packets;
using System;

namespace SSH.Processor
{
    public abstract class PacketProcessor : IPacketProcessor, IDisposable
    {
        public int PacketCount { get; set; }
        public StatusCode StatusCode { get; set; }
        public bool IsClosed { get; set; }

        internal Session session;
        private EventWaitHandle waitHandle;

        public PacketProcessor(Session session)
        {
            this.session = session;
            this.session.Processors.Add(this);
            waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public void ProcessPacket(object sender, ProcessPacketEventArgs e)
        {
            if (!e.Packet.Handled)
            {
                e.Packet.Handled = ProcessPacket(e.Packet);
            }
        }

        public bool ProcessPacket(IPacket p)
        {
            var result = InternalProcessPacket(p);
            if (result) PacketCount++;
            return result;
        }

        public abstract bool InternalProcessPacket(IPacket p);

        public virtual void Close()
        {
            Close(SSH.StatusCode.OK);
        }

        public virtual void Close(StatusCode code)
        {
            StatusCode = code;
            this.session.Processors.Remove(this);
            waitHandle.Set();
            IsClosed = true;
        }

        public void Wait()
        {
            waitHandle.WaitOne();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                waitHandle.Close();
            }
        }

        public class ProcessPacketEventArgs : EventArgs
        {
            public IPacket Packet { get; set; }

            public ProcessPacketEventArgs(IPacket packet)
            {
                this.Packet = packet;
            }
        }
    }
}
