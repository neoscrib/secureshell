namespace SSH.Packets
{
    class SshChannelRequestExec : SshChannelRequest
    {
        [SshProperty(4)]
        public string Command { get; set; }

        public SshChannelRequestExec() : base() { }

        public SshChannelRequestExec(uint channel, string command) :
            base(channel, "exec", true)
        {
            this.Command = command;
        }
    }
}
