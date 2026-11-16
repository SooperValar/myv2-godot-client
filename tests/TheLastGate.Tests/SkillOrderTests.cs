using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class SkillOrderTests
{
    private const uint SeyanDu = 1u << 1;
    private const uint Lycanthrope = 1u << 16;
    private const uint KnowsIdentify = 1u << 18;

    [Fact]
    public void Sorted_OrdersBySortKeyThenName()
    {
        var order = SkillOrder.Sorted(
            [Learnt(1, "Sword", 5), Learnt(2, "Dagger", 5), Learnt(3, "Axe", 2)],
            0);

        Assert.Equal([3, 2, 1], order);
    }

    [Fact]
    public void Sorted_PutsLearntSkillsFirst()
    {
        var order = SkillOrder.Sorted([Unlearnt(1, "Axe", 0), Learnt(2, "Sword", 9)], 0);

        Assert.Equal([2, 1], order);
    }

    [Fact]
    public void Sorted_PutsClassSkillsBeforeOtherSkills()
    {
        var order = SkillOrder.Sorted([Learnt(1, "Axe", 0), Learnt(50, "Zeal", 9)], 0);

        Assert.Equal([50, 1], order);
    }

    [Fact]
    public void Sorted_PutsIdentifyLastUnlessTheCharacterKnowsIt()
    {
        SkillEntry[] skills = [Learnt(52, "Identify", 0), Learnt(1, "Axe", 9)];

        Assert.Equal([1, 52], SkillOrder.Sorted(skills, 0));
        Assert.Equal([52, 1], SkillOrder.Sorted(skills, KnowsIdentify));
    }

    [Fact]
    public void Sorted_PutsTheLycanthropeSkillsLastForOthers()
    {
        SkillEntry[] skills = [Learnt(53, "Shift", 0), Learnt(1, "Axe", 9)];

        Assert.Equal([1, 53], SkillOrder.Sorted(skills, 0));
        Assert.Equal([53, 1], SkillOrder.Sorted(skills, Lycanthrope));
    }

    [Fact]
    public void Sorted_PutsSkillsThatWorkUnlearntFirstAmongTheUnlearnt()
    {
        var order = SkillOrder.Sorted([Unlearnt(1, "Axe", 0), Unlearnt(8, "Stealth", 9)], 0);

        Assert.Equal([8, 1], order);
    }

    [Fact]
    public void Sorted_PutsTheSeyanDuSkillLastForOthers()
    {
        SkillEntry[] skills = [Learnt(44, "Proximity", 0), Learnt(1, "Axe", 9)];

        Assert.Equal([1, 44], SkillOrder.Sorted(skills, 0));
        Assert.Equal([44, 1], SkillOrder.Sorted(skills, SeyanDu));
    }

    [Fact]
    public void Sorted_PutsNegativeSortKeysFirst()
    {
        var order = SkillOrder.Sorted([Learnt(1, "Axe", 1), Learnt(2, "Bow", -1)], 0);

        Assert.Equal([2, 1], order);
    }

    private static SkillEntry Learnt(int number, string name, int sortKey) => new(number, name, sortKey, true);

    private static SkillEntry Unlearnt(int number, string name, int sortKey) => new(number, name, sortKey, false);
}
