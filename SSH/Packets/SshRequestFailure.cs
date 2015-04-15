namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_REQUEST_FAILURE)]
    public class SshRequestFailure : Packet
    {
        public SshRequestFailure() : base(MessageCode.SSH_MSG_REQUEST_FAILURE) { }
    }
}
