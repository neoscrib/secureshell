namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_CHANNEL_REQUEST)]
    class SshChannelRequestPty : SshChannelRequest
    {
        [SshProperty(4)]
        public string Term { get; set; }
        [SshProperty(5)]
        public uint Columns { get; set; }
        [SshProperty(6)]
        public uint Rows { get; set; }
        [SshProperty(7)]
        public uint Width { get; set; }
        [SshProperty(8)]
        public uint Height { get; set; }
        [SshProperty(9)]
        public string Modes { get; set; }

        public SshChannelRequestPty() : base() { }

        public SshChannelRequestPty(uint channel, string term, uint termCols,
            uint termRows, uint termWidth, uint termHeight, string termModes)
            : base(channel, "pty-req", true)
        {
            this.Term = term;
            this.Columns = termCols;
            this.Rows = termRows;
            this.Width = termWidth;
            this.Height = termHeight;
            this.Modes = termModes;
        }
    }
}
