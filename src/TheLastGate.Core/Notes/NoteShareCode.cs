using System;

namespace TheLastGate.Notes;

public enum NoteCodeStatus
{
    Ok,
    NotACode,
    Damaged,
    TooLarge,
}

public static class NoteShareCode
{
    public const int DiscordLimit = 2000;
    public const string TooLongForDiscord = "Too long for one Discord message; it will still work on the wiki.";

    public static bool HasPrefix(string text) => throw new NotImplementedException();

    public static string? Export(NoteSet set, int categoryId) => throw new NotImplementedException();

    public static NoteCodeStatus Import(string code, out NoteSet? imported) => throw new NotImplementedException();

    public static string Describe(NoteSet imported) => throw new NotImplementedException();

    public static string Problem(NoteCodeStatus status) => throw new NotImplementedException();

    public static string CopiedMessage(int characters) => throw new NotImplementedException();
}
