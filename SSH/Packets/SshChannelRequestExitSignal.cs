namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    public class SshChannelRequestExitSignal : SshChannelRequest
    {
        [SshProperty(4)]
        public string SignalName { get; set; }

        public SshChannelRequestExitSignal() : base() { }
    }
}
