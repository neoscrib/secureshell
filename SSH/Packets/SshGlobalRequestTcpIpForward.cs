using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSH.Packets
{
    //[Packet(MessageCode.SSH_MSG_GLOBAL_REQUEST)]
    class SshGlobalRequestTcpIpForward : SshGlobalRequest
    {
        [SshProperty(3)]
        public string RemoteAddress { get; set; }
        [SshProperty(4)]
        public uint RemotePort { get; set; }

        public SshGlobalRequestTcpIpForward(string remoteAddress, uint remotePort, bool wantReply)
            : base("tcpip-forward", wantReply)
        {
            this.RemoteAddress = remoteAddress;
            this.RemotePort = remotePort;
        }
    }
}
