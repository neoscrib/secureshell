using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSH.Packets;

namespace SSH.Processor
{
    interface IChannelProcessor : IPacketProcessor
    {
        uint LocalChannel { get; set; }
        uint RemoteChannel { get; set; }
        ChannelProcessor.ChannelType Type { get; set; }
        uint ExitCode { get; set; }

        //bool InternalProcessPacket(ISshChannelMessage p);
        void OnChannelOpenConfirmation(ISshChannelMessage p);
        void OnChannelOpenFailure(ISshChannelMessage p);
        void OnChannelSuccess(ISshChannelMessage p);
        void OnChannelWindowAdjust(ISshChannelMessage p);
        void OnChannelData(ISshChannelMessage p);
        void OnChannelExtendedData(ISshChannelMessage p);
        void OnChannelClose(ISshChannelMessage p);
        void OnChannelEndOfFile(ISshChannelMessage p);
        void OnChannelRequest(ISshChannelMessage p);
    }
}
