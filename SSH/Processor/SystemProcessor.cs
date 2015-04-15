using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SSH.Packets;

namespace SSH.Processor
{
    class SystemProcessor : PacketProcessor, IPacketProcessor
    {
        public SystemProcessor(Session session) : base(session) { }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_IGNORE:
                    return true;
                case MessageCode.SSH_MSG_DEBUG:
                    return true;
                case MessageCode.SSH_MSG_DISCONNECT:
                    {
                        var msg = (SshDisconnect)p;
                        switch (msg.ReasonCode)
                        {
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_TOO_MANY_CONNECTIONS:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_SERVICE_NOT_AVAILABLE:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_RESERVED:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_PROTOCOL_VERSION_NOT_SUPPORTED:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_PROTOCOL_ERROR:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_MAC_ERROR:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_KEY_EXCHANGE_FAILED:
                                Close(SSH.StatusCode.ProtocolError);
                                break;
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_NO_MORE_AUTH_METHODS_AVAILABLE:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_ILLEGAL_USER_NAME:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_AUTH_CANCELLED_BY_USER:
                                Close(SSH.StatusCode.AuthenticationFailed);
                                break;
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_BY_APPLICATION:
                                Close(SSH.StatusCode.OK);
                                break;
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_CONNECTION_LOST:
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_COMPRESSION_ERROR:
                                Close(SSH.StatusCode.ProtocolError);
                                break;
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_HOST_KEY_NOT_VERIFIABLE:
                                Close(SSH.StatusCode.HostVerificationFailed);
                                break;
                            case SshDisconnect.SshDisconnectReason.SSH_DISCONNECT_HOST_NOT_ALLOWED_TO_CONNECT:
                                Close(SSH.StatusCode.HostNotAllowed);
                                break;

                        }
                        session.Disconnect(msg.ReasonCode, msg.Reason);
                    }
                    return true;
                case MessageCode.SSH_MSG_GLOBAL_REQUEST:
                    {
                        var msg = (SshGlobalRequest)p;
                        if (msg.RequestName == "keepalive@openssh.com")
                        {
                            session.Socket.WritePacket(new SshRequestSuccess());
                        }
                        else
                        {
                            if (msg.WantReply)
                            {
                                //if (msg.RequestName == "keepalive@openssh.com")
                                //    session.Socket.WritePacket(new SshRequestSuccess());
                                //else
                                session.Socket.WritePacket(new SshRequestFailure());
                            }
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
