namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    class SshChannelRequestSftp : SshChannelRequest
    {
        [SshProperty(4)]
        public string Subsystem { get { return "sftp"; } }

        public SshChannelRequestSftp() : base() { }

        public SshChannelRequestSftp(uint channel) :
            base(channel, "subsystem", true)
        { }
    }
}
