namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    class SshChannelRequestKeepAlive : SshChannelRequest
    {
        public SshChannelRequestKeepAlive() : base() { }
    }
}
