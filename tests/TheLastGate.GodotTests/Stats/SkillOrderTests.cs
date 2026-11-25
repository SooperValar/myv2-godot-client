using GdUnit4;
using TheLastGate.Stats;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Stats;

[TestSuite]
public class SkillOrderTests
{
    [TestCase]
    public void Sorted_PutsLearntSkillsBeforeUnlearntOnes()
    {
        var order = SkillOrder.Sorted([Unlearnt(1, "Axe", 'C'), Learnt(2, "Sword", 'R')]);

        AssertThat(order).ContainsExactly(2, 1);
    }

    [TestCase]
    public void Sorted_PutsFixedSkillsBelowRaisableOnes()
    {
        var order = SkillOrder.Sorted([Fixed(30, "Meditate", 'B'), Learnt(3, "Sword", 'C')]);

        AssertThat(order).ContainsExactly(3, 30);
    }

    [TestCase]
    public void Sorted_OrdersByGroupThenName()
    {
        var order = SkillOrder.Sorted(
            [Learnt(26, "Heal", 'R'), Learnt(3, "Sword", 'C'), Learnt(4, "Axe", 'C'), Learnt(8, "Stealth", 'G')]);

        AssertThat(order).ContainsExactly(4, 3, 8, 26);
    }

    [TestCase]
    public void Sorted_PutsUnnamedSlotsLast()
    {
        var order = SkillOrder.Sorted([new SkillEntry(40, "", 'Z', false, false), Unlearnt(1, "Axe", 'C')]);

        AssertThat(order).ContainsExactly(1, 40);
    }

    private static SkillEntry Learnt(int number, string name, char group) => new(number, name, group, true, false);

    private static SkillEntry Unlearnt(int number, string name, char group) => new(number, name, group, false, false);

    private static SkillEntry Fixed(int number, string name, char group) => new(number, name, group, true, true);
}
