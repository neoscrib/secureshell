using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Packets
{
    public interface ISshChannelMessage : IPacket
    {
        uint Channel { get; set; }
    }
}
