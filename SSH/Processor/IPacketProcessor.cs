using SSH.Packets;
using System.Threading;

namespace SSH.Processor
{
    public interface IPacketProcessor
    {
        bool IsClosed { get; set; }
        int PacketCount { get; set; }
        StatusCode StatusCode { get; set; }
        void ProcessPacket(object sender, PacketProcessor.ProcessPacketEventArgs e);
        bool ProcessPacket(IPacket p);
        void Close(StatusCode code);
        void Wait();
        void Close();
    }
}
