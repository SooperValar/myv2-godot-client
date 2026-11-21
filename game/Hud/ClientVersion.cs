namespace TheLastGate.Hud;

public static class ClientVersion
{
    private const string Unreleased = "0.0.0";

    // .NET appends "+commit" to the version it builds in; until the first release the version stays at 0.0.0.
    public static string Text(string version)
    {
        var released = version.Split('+')[0];
        return $"Client Version {(released is "" or Unreleased ? "RC" : released)}";
    }
}
