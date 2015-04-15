using System.Threading;
using SSH.Packets;
using System;

namespace SSH.Processor
{
    public abstract class PacketProcessor : IPacketProcessor, IDisposable
    {
        public int PacketCount { get; set; }
        public StatusCode StatusCode { get; set; }

        internal Session session;
        private EventWaitHandle waitHandle;

        public PacketProcessor(Session session)
        {
            this.session = session;
            this.session.Processors.Add(this);
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
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
        }

        public void Wait()
        {
            waitHandle.WaitOne();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
