namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_GLOBAL_REQUEST)]
    public class SshGlobalRequest : Packet
    {
        [SshProperty(1)]
        public string RequestName { get; set; }
        [SshProperty(2)]
        public bool WantReply { get; set; }

        public SshGlobalRequest() : base(MessageCode.SSH_MSG_GLOBAL_REQUEST) { }

        public SshGlobalRequest(string requestName, bool wantReply)
            : base(MessageCode.SSH_MSG_GLOBAL_REQUEST)
        {
            this.RequestName = requestName;
            this.WantReply = wantReply;
        }
    }
}
