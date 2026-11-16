using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class StatPlanTests
{
    private const int Plenty = 1_000_000;
    private const int Dagger = 3;

    [Fact]
    public void NextCost_OfAnAttributeIsCubicInTheValueAfterPlannedRaises()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();

        Assert.Equal(150, plan.NextCost(0, values));
        plan.Raise(0, values, Plenty, 1);
        Assert.Equal(199, plan.NextCost(0, values));
    }

    [Theory]
    [InlineData(StatPlan.HitPointsSlot)]
    [InlineData(StatPlan.ManaSlot)]
    public void NextCost_OfHitPointsAndManaIsLinear(int slot)
    {
        Assert.Equal(400, new StatPlan().NextCost(slot, [100, 0, 500, 4, 0, 1250]));
    }

    [Fact]
    public void NextCost_OfASkillIsAtLeastItsValue()
    {
        Assert.Equal(3, new StatPlan().NextCost(StatPlan.SkillSlot(Dagger), [3, 0, 50, 1, 0, 3]));
    }

    [Fact]
    public void NextCost_IsNullAtTheLimit()
    {
        int[] values = [49, 0, 50, 3, 0, 12];
        var plan = new StatPlan();
        plan.Raise(0, values, Plenty, 1);

        Assert.Null(plan.NextCost(0, values));
    }

    [Fact]
    public void NextCost_IsNullForAnUnlearntSkill()
    {
        Assert.Null(new StatPlan().NextCost(StatPlan.SkillSlot(Dagger), [0, 0, 50, 2, 0, 6]));
    }

    [Fact]
    public void Raise_PlansThePointsAndAddsUpTheirCost()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();

        plan.Raise(0, values, Plenty, 2);

        Assert.Equal(2, plan.Raised(0));
        Assert.Equal(150 + 199, plan.PointsUsed);
    }

    [Fact]
    public void Raise_StopsWhenThePointsLeftDoNotCoverTheNextPoint()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();

        plan.Raise(0, values, 400, 10);

        Assert.Equal(2, plan.Raised(0));
    }

    [Fact]
    public void Raise_StopsAtTheLimit()
    {
        int[] values = [48, 0, 50, 3, 0, 12];
        var plan = new StatPlan();

        plan.Raise(0, values, Plenty, 10);

        Assert.Equal(2, plan.Raised(0));
    }

    [Fact]
    public void Lower_RefundsTheLastPlannedPoint()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();
        plan.Raise(0, values, Plenty, 2);

        plan.Lower(0, values, 1);

        Assert.Equal(1, plan.Raised(0));
        Assert.Equal(150, plan.PointsUsed);
    }

    [Fact]
    public void Lower_StopsAtNothingPlanned()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();
        plan.Raise(0, values, Plenty, 1);

        plan.Lower(0, values, 10);

        Assert.Equal(0, plan.Raised(0));
        Assert.Equal(0, plan.PointsUsed);
    }

    [Fact]
    public void Commit_SendsOneStatCommandPerPlannedSlotInSlotOrder()
    {
        var plan = new StatPlan();
        plan.Raise(StatPlan.SkillSlot(Dagger), [10, 0, 50, 2, 0, 20], Plenty, 1);
        plan.Raise(StatPlan.ManaSlot, [60, 0, 500, 3, 0, 900], Plenty, 3);
        plan.Raise(2, [10, 0, 50, 3, 0, 12], Plenty, 2);

        Assert.Equal(
            [
                ClientPackets.ShortAndInt(ClientCommand.Stat, 2, 2),
                ClientPackets.ShortAndInt(ClientCommand.Stat, StatPlan.ManaSlot, 3),
                ClientPackets.ShortAndInt(ClientCommand.Stat, StatPlan.FirstSkillSlot + Dagger, 1),
            ],
            plan.Commit());
    }

    [Fact]
    public void Commit_SplitsSkillRaisesIntoAmountsTheServerAccepts()
    {
        var plan = new StatPlan();
        plan.Raise(StatPlan.SkillSlot(Dagger), [1, 0, 300, 0, 0, 1], Plenty, 200);

        Assert.Equal(
            [
                ClientPackets.ShortAndInt(ClientCommand.Stat, StatPlan.SkillSlot(Dagger), 135),
                ClientPackets.ShortAndInt(ClientCommand.Stat, StatPlan.SkillSlot(Dagger), 65),
            ],
            plan.Commit());
    }

    [Fact]
    public void Commit_ClearsThePlan()
    {
        int[] values = [10, 0, 50, 3, 0, 12];
        var plan = new StatPlan();
        plan.Raise(0, values, Plenty, 1);

        plan.Commit();

        Assert.Equal(0, plan.Raised(0));
        Assert.Equal(0, plan.PointsUsed);
        Assert.Empty(plan.Commit());
    }

    [Fact]
    public void Values_FindsTheServerValuesForEachSlot()
    {
        var player = new PlayerState();

        Assert.Same(player.Attributes[2], StatPlan.Values(player, 2));
        Assert.Same(player.HitPoints, StatPlan.Values(player, StatPlan.HitPointsSlot));
        Assert.Same(player.Mana, StatPlan.Values(player, StatPlan.ManaSlot));
        Assert.Same(player.Skills[Dagger], StatPlan.Values(player, StatPlan.SkillSlot(Dagger)));
    }

    [Theory]
    [InlineData(false, false, 1)]
    [InlineData(true, false, 10)]
    [InlineData(false, true, 90)]
    [InlineData(true, true, 90)]
    public void ClickTimes_RaisesTenWithShiftAndNinetyWithCtrlOrAlt(bool shift, bool ctrlOrAlt, int times)
    {
        Assert.Equal(times, StatPlan.ClickTimes(shift, ctrlOrAlt));
    }

    [Fact]
    public void Help_NamesTheStatBeingRaisedOrLowered()
    {
        Assert.Equal("Raise Dagger.", StatPlan.RaiseHelp("Dagger"));
        Assert.Equal("Lower Mana.", StatPlan.LowerHelp("Mana"));
    }
}
