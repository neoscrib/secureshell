namespace SSH.Packets
{
    [Packet(MessageCode.SSH_MSG_USERAUTH_SUCCESS)]
    public class SshUserAuthSuccess : Packet
    {
        public SshUserAuthSuccess()
            : base(MessageCode.SSH_MSG_USERAUTH_SUCCESS)
        { }
    }
}
