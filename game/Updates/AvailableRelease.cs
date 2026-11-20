using System;

namespace TheLastGate.Updates;

public sealed record AvailableRelease(Version Version, string NotesMarkdown, long FullSizeBytes)
{
    public ReleaseNotes Notes => ReleaseNotes.Parse(NotesMarkdown);
}
