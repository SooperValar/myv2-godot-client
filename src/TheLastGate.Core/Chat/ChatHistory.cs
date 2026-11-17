using System;

namespace TheLastGate.Chat;

// Entry 0 holds the line being typed while older lines are shown, so moving back down returns to it.
public sealed class ChatHistory
{
    private const int Entries = 20;

    private readonly string[] entries = CreateEntries();
    private int position;
    private int sentCount;

    public string? Older(string current)
    {
        if (position >= sentCount)
        {
            return null;
        }

        entries[position] = current;
        position++;
        return entries[position];
    }

    public string? Newer(string current)
    {
        if (position == 0)
        {
            return null;
        }

        entries[position] = current;
        position--;
        return entries[position];
    }

    public void Sent(string line)
    {
        Array.Copy(entries, 1, entries, 2, Entries - 2);
        entries[1] = line;
        entries[0] = "";
        position = 0;
        sentCount = Math.Min(sentCount + 1, Entries - 1);
    }

    public void Abandon()
    {
        entries[0] = "";
        position = 0;
    }

    private static string[] CreateEntries()
    {
        var created = new string[Entries];
        Array.Fill(created, "");
        return created;
    }
}
