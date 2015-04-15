namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    public class SshChannelRequestExitStatus : SshChannelRequest
    {
        [SshProperty(4)]
        public uint ExitStatus { get; set; }

        public SshChannelRequestExitStatus() : base() { }
    }
}
