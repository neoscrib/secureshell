using SSH.Packets;
using System.Diagnostics;

namespace SSH.Processor
{
    class IdentificationProcessor : PacketProcessor, IPacketProcessor
    {
        public IdentificationProcessor(Session session)
            : base(session)
        { }

        public new void Close(StatusCode code)
        {
            base.Close(code);
            if (code == SSH.StatusCode.NotSSH20)
                session.Disconnect(SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_PROTOCOL_VERSION_NOT_SUPPORTED);
        }

        public override bool InternalProcessPacket(IPacket p)
        {
            if (p.Code == MessageCode.SSH_MSG_IDENTIFICATION)
            {
                session.ServerIdentifier = (SshIdentification)p;
                if (!session.ServerIdentifier.Identifier.StartsWith("SSH-2.0"))
                {
                    this.Close(SSH.StatusCode.NotSSH20);
                }
                else
                {
                    session.ClientIdentifier = new SshIdentification();
                    session.Socket.WritePacket(session.ClientIdentifier);
                    this.Close(StatusCode.OK);
                }
                return true;
            }

            return false;
        }
    }
}
