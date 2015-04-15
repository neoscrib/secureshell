using SSH.Packets;
using System.Threading;

namespace SSH.Processor
{
    public interface IPacketProcessor
    {
        int PacketCount { get; set; }
        StatusCode StatusCode { get; set; }
        bool ProcessPacket(IPacket p);
        void Close(StatusCode code);
        void Wait();
        void Close();
    }
}
