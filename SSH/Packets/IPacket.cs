using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Packets
{
    public interface IPacket
    {
        bool Handled { get; set; }
        MessageCode Code { get; set; }
        byte[] ToSshMessage();
    }
}
