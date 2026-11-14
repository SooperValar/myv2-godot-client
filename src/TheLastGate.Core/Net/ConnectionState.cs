namespace TheLastGate.Net;

public enum ConnectionState
{
    Idle,
    Connecting,
    LoggingIn,
    Connected,
    Closed,
    Failed,
}
