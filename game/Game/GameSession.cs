using TheLastGate.Characters;
using TheLastGate.Net;

namespace TheLastGate.Game;

public sealed record GameSession(
    CharacterRecord Record,
    LoginHandshake Handshake,
    ServerAddress Server,
    ServerConnection Connection)
{
    private static GameSession? pending;

    public static void Begin(GameSession session) => pending = session;

    public static GameSession? Take()
    {
        var session = pending;
        pending = null;
        return session;
    }
}
