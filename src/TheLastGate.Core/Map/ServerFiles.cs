using System.Linq;

namespace TheLastGate.Map;

// One file per server, shared by every character, named the way the current client names it so its files can be
// found and read.
public static class ServerFiles
{
    public static string Name(string prefix, string host, int port)
    {
        var safeHost = new string(host.Select(character => IsSafe(character) ? character : '_').ToArray());
        return $"{prefix}{safeHost}_{port}.dat";
    }

    private static bool IsSafe(char character) =>
        char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_';
}
