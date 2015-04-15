﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SSH.Processor;
using SSH.Packets;
using System.Threading;
using SSH.Threading;
using System.Net.Sockets;

namespace SSH
{
    public class RemoteForward : PacketProcessor, IDisposable
    {
        private string localAddress;
        private uint localPort;
        private string remoteAddress;
        private uint remotePort;

        public RemoteForward(Session session, string localAddress, uint localPort, string remoteAddress, uint remotePort)
            : base(session)
        {
            session.Disposables.Add(this);
            this.localAddress = localAddress;
            this.localPort = localPort;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;

            session.Socket.WritePacket(new SshGlobalRequestTcpIpForward(remoteAddress, remotePort, false));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool InternalProcessPacket(IPacket p)
        {
            if (p is SshChannelOpenForwardedTcpIp)
            {
                var msg = (SshChannelOpenForwardedTcpIp)p;
                if (msg.RemoteAddress == this.remoteAddress && msg.RemotePort == this.remotePort)
                {
                    TcpClient client = new TcpClient();
                    client.Connect(localAddress, (int)localPort);
                    new LocalForward(this.session, client, msg.Channel); 
                    return true;
                }
            }
            return false;
        }
    }
}
