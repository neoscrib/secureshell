namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_SERVICE_REQUEST)]
    public class SshServiceRequest : Packet
    {
        [SshProperty(1)]
        public string ServiceName { get; set; }

        public SshServiceRequest() : base(MessageCode.SSH_MSG_SERVICE_REQUEST) { }

        public SshServiceRequest(string serviceName)
            : this()
        {
            ServiceName = serviceName;
        }
    }
}
