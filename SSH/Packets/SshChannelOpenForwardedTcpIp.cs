using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_OPEN)]
    class SshChannelOpenForwardedTcpIp : SshChannelOpen
    {
        [SshProperty(5)]
        public string RemoteAddress { get; set; }
        [SshProperty(6)]
        public uint RemotePort { get; set; }
        [SshProperty(7)]
        public string ClientAddress { get; set; }
        [SshProperty(8)]
        public uint ClientPort { get; set; }

        public SshChannelOpenForwardedTcpIp() : base() { }
    }
}
