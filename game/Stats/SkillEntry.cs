namespace TheLastGate.Stats;

public readonly record struct SkillEntry(int Number, string Name, int SortKey, bool Learnt, bool Fixed)
{
    private const int BaseValue = 0;
    private const int Limit = 2;
    private const int FixedLimit = 1;

    public static SkillEntry For(int number, string name, int sortKey, int[] values) =>
        new(number, name, sortKey, values[BaseValue] != 0, values[BaseValue] != 0 && values[Limit] <= FixedLimit);
}
