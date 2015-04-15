namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    public class SshChannelRequest : Packet, ISshChannelMessage
    {
        [SshProperty(1)]
        public uint Channel { get; set; }
        [SshProperty(2)]
        public string RequestType { get; set; }
        [SshProperty(3)]
        public bool WantReply { get; set; }

        public SshChannelRequest() : base(MessageCode.SSH_MSG_CHANNEL_REQUEST) { }

        public SshChannelRequest(uint channel, string requestType, bool wantReply)
            : this()
        {
            this.Channel = channel;
            this.RequestType = requestType;
            this.WantReply = wantReply;
        }
    }
}
