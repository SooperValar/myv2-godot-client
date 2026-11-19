using System.Collections.Generic;

namespace TheLastGate.Updates;

public sealed record NotesSection(string Title, IReadOnlyList<string> Items);
