using System.Collections.Generic;

namespace TheLastGate.State;

public sealed class CharacterNames
{
    private readonly Dictionary<int, (int Id, string Name)> namesByNumber = new();

    public string NameOf(int number, int id) =>
        namesByNumber.TryGetValue(number, out var entry) && (id == 0 || id == entry.Id) ? entry.Name : "";

    internal void Remember(int number, int id, string name) => namesByNumber[number] = (id, name);
}
