namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_REQUEST_SUCCESS)]
    public class SshRequestSuccess : Packet
    {
        public SshRequestSuccess() : base(MessageCode.SSH_MSG_REQUEST_SUCCESS) { }
    }
}
