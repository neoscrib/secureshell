namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_SERVICE_ACCEPT)]
    public class SshServiceAccept : Packet
    {
        [SshProperty(1)]
        public string ServiceName { get; set; }

        public SshServiceAccept() : base(MessageCode.SSH_MSG_SERVICE_ACCEPT) { }

        public SshServiceAccept(string serviceName)
            : this()
        {
            ServiceName = serviceName;
        }
    }
}
