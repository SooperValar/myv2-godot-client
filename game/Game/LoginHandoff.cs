using TheLastGate.Characters;
using TheLastGate.Net;

namespace TheLastGate.Game;

public sealed record LoginHandoff(
    CharacterRecord Record,
    LoginHandshake Handshake,
    ServerAddress Server,
    ServerConnection Connection)
{
    private static LoginHandoff? pending;

    public static void Begin(LoginHandoff session) => pending = session;

    public static LoginHandoff? Take()
    {
        var session = pending;
        pending = null;
        return session;
    }
}
