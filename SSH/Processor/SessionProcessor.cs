using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SSH.Packets;

namespace SSH.Processor
{
    public class SessionProcessor : List<IPacketProcessor>
    {
        public int PacketCount { get; set; }

        private Session session;
        //private EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
        //private List<IPacketProcessor> add = new List<IPacketProcessor>();
        //private List<IPacketProcessor> remove = new List<IPacketProcessor>();

        internal delegate void PacketReceivedEventHandler(object sender, PacketProcessor.ProcessPacketEventArgs e);
        internal event PacketReceivedEventHandler PacketReceived;

        public SessionProcessor(Session session)
        {
            this.session = session;
        }

        public new void Add(IPacketProcessor processor)
        {
            this.PacketReceived += processor.ProcessPacket;
            base.Add(processor);
            //waitHandle.WaitOne();
            //add.Add(processor);
            //waitHandle.Set();
        }

        public new bool Remove(IPacketProcessor processor)
        {
            this.PacketReceived -= processor.ProcessPacket;
            return base.Remove(processor);
            //waitHandle.WaitOne();
            //remove.Add(processor);
            //waitHandle.Set();
            //return true;
        }

        public void Process(IPacket p)
        {
            //waitHandle.WaitOne();
            //this.AddRange(add);
            //add.Clear();
            //waitHandle.Set();

            //bool handled = this.Any(processor => processor.ProcessPacket(p));
            PacketReceived(session, new PacketProcessor.ProcessPacketEventArgs(p));

            //waitHandle.WaitOne();
            //this.RemoveAll(p1 => remove.Contains(p1));
            //remove.Clear();
            //waitHandle.Set();

            if (!p.Handled)
                throw new Exception("A packet failed to be handled by any processor.");
            if (session.SystemProcessor.IsClosed)
                session.Disconnect();
            //if (!this.Any(processor => processor is SystemProcessor))
            //    session.Disconnect();

            if (++PacketCount % 500 == 0)
                this.Sort((a, b) => b.PacketCount.CompareTo(a.PacketCount));
        }
    }
}
