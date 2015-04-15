﻿namespace SSH.Packets
{
    public enum SftpStatusCode : uint
    {
        SSH_FX_OK = 0,
        SSH_FX_EOF = 1,
        SSH_FX_NO_SUCH_FILE = 2,
        SSH_FX_PERMISSION_DENIED = 3,
        SSH_FX_FAILURE = 4,
        SSH_FX_BAD_MESSAGE = 5,
        SSH_FX_NO_CONNECTION = 6,
        SSH_FX_CONNECTION_LOST = 7,
        SSH_FX_OP_UNSUPPORTED = 8
    }
}
