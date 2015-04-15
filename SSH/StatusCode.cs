using System.ComponentModel;

namespace SSH
{
    public enum StatusCode
    {
        [Description("OK")]
        OK,
        [Description("Only SSH-2.0 is supported.")]
        NotSSH20,
        [Description("Authentication Failed!")]
        AuthenticationFailed,
        [Description("The signature could not be verified!")]
        SignatureVerificationFailed,
        [Description("Host couldn't be verified!")]
        HostVerificationFailed,
        [Description("Protocol error!")]
        ProtocolError,
        [Description("Host not allowed to connect!")]
        HostNotAllowed,
        [Description("Connection timed out!")]
        ConnectionTimedOut = 10060,
        [Description("Connection refused!")]
        ConnectionRefused = 10061
    }
}
