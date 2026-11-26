using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheLastGate.Launcher;

public sealed class CharacterHistory
{
    public const int MaxEntries = 20;

    private readonly List<PreviousCharacter> entries = [];

    public CharacterHistory()
    {
    }

    public CharacterHistory(IEnumerable<PreviousCharacter> mostRecentFirst)
    {
        foreach (var entry in mostRecentFirst)
        {
            if (entries.Count < MaxEntries && IndexOf(entry.FilePath) < 0)
            {
                entries.Add(entry);
            }
        }
    }

    public IReadOnlyList<PreviousCharacter> Entries => entries;

    public void Use(string name, string filePath)
    {
        var knownIndex = IndexOf(filePath);
        if (knownIndex >= 0)
        {
            entries.RemoveAt(knownIndex);
        }

        entries.Insert(0, new PreviousCharacter(name, filePath));
        if (entries.Count > MaxEntries)
        {
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
        }
    }

    public string Label(PreviousCharacter entry)
    {
        var fileName = Path.GetFileNameWithoutExtension(entry.FilePath);
        if (entry.Name.Length == 0)
        {
            return fileName;
        }

        return entries.Count(other => other.Name == entry.Name) > 1 ? $"{entry.Name} ({fileName})" : entry.Name;
    }

    public void Remove(string filePath) => entries.RemoveAll(entry => SameFile(entry.FilePath, filePath));

    public void Clear() => entries.Clear();

    public void RemoveMissingFiles() => entries.RemoveAll(entry => !File.Exists(entry.FilePath));

    private int IndexOf(string filePath) => entries.FindIndex(entry => SameFile(entry.FilePath, filePath));

    private static bool SameFile(string first, string second) =>
        string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
}
