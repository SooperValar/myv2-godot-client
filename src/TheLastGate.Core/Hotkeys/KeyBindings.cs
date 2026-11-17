using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Hotkeys;

public sealed class KeyBindings
{
    private const string NotSet = "Not Set";

    private readonly Dictionary<string, KeyChord?> current =
        Bindings.All.ToDictionary(binding => binding.Id, binding => binding.Default);

    public KeyChord? For(string id) => current[id];

    public string LongName(string id) => For(id)?.LongName ?? NotSet;

    public string ShortName(string id) => For(id)?.ShortName ?? "";

    public Binding? Find(KeyChord pressed) =>
        Bindings.All.FirstOrDefault(binding => !binding.Hold && current[binding.Id] == pressed);

    // A key does one thing, so the binding that had it loses it; its id is returned to tell the player.
    public string? Assign(string id, KeyChord chord)
    {
        var displaced = Bindings.All.FirstOrDefault(binding => binding.Id != id && current[binding.Id] == chord)?.Id;
        if (displaced != null)
        {
            current[displaced] = null;
        }

        current[id] = chord;
        return displaced;
    }

    public void Clear(string id) => current[id] = null;

    public void Reset(string id) => current[id] = Bindings.All.First(binding => binding.Id == id).Default;

    public void ResetAll()
    {
        foreach (var binding in Bindings.All)
        {
            current[binding.Id] = binding.Default;
        }
    }

    // An empty value unbinds; an unknown id, or a value that can't be read, keeps the default.
    public void Load(IReadOnlyDictionary<string, string> saved)
    {
        foreach (var (id, value) in saved)
        {
            if (!current.ContainsKey(id))
            {
                continue;
            }

            if (value.Length == 0)
            {
                current[id] = null;
            }
            else if (KeyChord.Parse(value) is { } chord)
            {
                current[id] = chord;
            }
        }
    }

    public IReadOnlyDictionary<string, string> Saved() =>
        Bindings.All.ToDictionary(binding => binding.Id, binding => current[binding.Id]?.LongName ?? "");
}
