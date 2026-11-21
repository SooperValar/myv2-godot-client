namespace TheLastGate.Net;

public static class ExitReasons
{
    public const int EnteredTavern = 10;
    public const int UserRequest = 12;

    private static readonly string[] Reasons =
    [
        "unknown",
        "Client failed challenge.",
        "Client was idle too long.",
        "No room to drop character.",
        "Invalid parameters.",
        "Character already active or no player character.",
        "Invalid password.",
        "Client too slow.",
        "Receive failure.",
        "Server is being shutdown.",
        "You entered a Tavern.",
        "Client version too old. Update needed.",
        "Aborting on user request.",
        "this should never show up",
        "You have been banned for an hour. Enhance your social behavior before you come back.",
    ];

    public static string Describe(uint code) => code >= 1 && code < Reasons.Length ? Reasons[code] : "unknown reason";
}
