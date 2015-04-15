﻿namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_DISCONNECT)]
    public class SshDisconnect : Packet
    {
        [SshProperty(1)]
        public SshDisconnectReason ReasonCode { get; set; }
        [SshProperty(2)]
        public string Reason { get; set; }
        [SshProperty(3)]
        public string Language { get; set; }

        public SshDisconnect() : base(MessageCode.SSH_MSG_DISCONNECT) { }

        public SshDisconnect(SshDisconnectReason reasonCode, string reasonDescription)
            : this()
        {
            ReasonCode = reasonCode;
            Reason = reasonDescription;
            Language = string.Empty;
        }

        public enum SshDisconnectReason : uint
        {
            SSH_DISCONNECT_HOST_NOT_ALLOWED_TO_CONNECT = 1,
            SSH_DISCONNECT_PROTOCOL_ERROR = 2,
            SSH_DISCONNECT_KEY_EXCHANGE_FAILED = 3,
            SSH_DISCONNECT_RESERVED = 4,
            SSH_DISCONNECT_MAC_ERROR = 5,
            SSH_DISCONNECT_COMPRESSION_ERROR = 6,
            SSH_DISCONNECT_SERVICE_NOT_AVAILABLE = 7,
            SSH_DISCONNECT_PROTOCOL_VERSION_NOT_SUPPORTED = 8,
            SSH_DISCONNECT_HOST_KEY_NOT_VERIFIABLE = 9,
            SSH_DISCONNECT_CONNECTION_LOST = 10,
            SSH_DISCONNECT_BY_APPLICATION = 11,
            SSH_DISCONNECT_TOO_MANY_CONNECTIONS = 12,
            SSH_DISCONNECT_AUTH_CANCELLED_BY_USER = 13,
            SSH_DISCONNECT_NO_MORE_AUTH_METHODS_AVAILABLE = 14,
            SSH_DISCONNECT_ILLEGAL_USER_NAME = 15
        }
    }
}
