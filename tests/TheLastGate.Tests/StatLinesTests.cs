using TheLastGate.Assets;
using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class StatLinesTests
{
    private const int Plenty = 1_000_000;

    [Theory]
    [InlineData(0, "Braveness")]
    [InlineData(1, "Willpower")]
    [InlineData(2, "Intuition")]
    [InlineData(3, "Agility")]
    [InlineData(4, "Strength")]
    public void Attribute_NamesTheFiveAttributes(int attribute, string name)
    {
        Assert.Equal(name, StatLines.Attribute(attribute, [10, 0, 50, 3, 0, 12], Plenty).Name);
    }

    [Fact]
    public void Attribute_ShowsTheBaseTheScoreAndTheCubicCost()
    {
        var line = StatLines.Attribute(0, [10, 0, 50, 3, 1, 44], Plenty);

        Assert.Equal(new StatLine("Braveness", TextColour.Yellow, 10, 300, true, 150), line);
    }

    [Theory]
    [InlineData(150, true)]
    [InlineData(149, false)]
    public void Attribute_OffersARaiseOnlyWhenThePointsCoverIt(int points, bool canRaise)
    {
        Assert.Equal(canRaise, StatLines.Attribute(0, [10, 0, 50, 3, 0, 12], points).CanRaise);
    }

    [Fact]
    public void Attribute_HasNoCostAtItsLimit()
    {
        var line = StatLines.Attribute(0, [50, 0, 50, 3, 0, 55], Plenty);

        Assert.Null(line.Cost);
        Assert.False(line.CanRaise);
    }

    [Fact]
    public void HitPoints_ShowsTheBaseTheMaximumAndALinearCost()
    {
        var line = StatLines.HitPoints([100, 0, 500, 4, 0, 1250], Plenty);

        Assert.Equal(new StatLine("Hitpoints", TextColour.Yellow, 100, 1250, true, 400), line);
    }

    [Fact]
    public void Mana_ShowsTheBaseTheMaximumAndALinearCost()
    {
        var line = StatLines.Mana([60, 0, 500, 3, 0, 900], Plenty);

        Assert.Equal(new StatLine("Mana", TextColour.Yellow, 60, 900, true, 180), line);
    }

    [Fact]
    public void Mana_HasNoCostAtItsLimit()
    {
        Assert.Null(StatLines.Mana([500, 0, 500, 3, 0, 900], Plenty).Cost);
    }

    [Fact]
    public void Skill_ShowsADashForAHiddenSkill()
    {
        Assert.Equal(StatLine.Unknown, StatLines.Skill("Dagger", 0, [10, 0, 50, 2, 0, 20], Plenty));
    }

    [Fact]
    public void Skill_ShowsAKnownSkillsBaseScoreAndCost()
    {
        var line = StatLines.Skill("Dagger", 1, [10, 0, 50, 2, 0, 20], Plenty);

        Assert.Equal(new StatLine("Dagger", TextColour.Yellow, 10, 20, true, 50), line);
    }

    [Fact]
    public void Skill_CostsAtLeastItsValue()
    {
        Assert.Equal(3, StatLines.Skill("Dagger", 1, [3, 0, 50, 1, 0, 3], Plenty).Cost);
    }

    [Fact]
    public void Skill_HidesTheBaseAndCostOfAnUnlearntSkillButStillOffersTheRaise()
    {
        var line = StatLines.Skill("Stealth", 1, [0, 0, 50, 2, 0, 6], Plenty);

        Assert.Equal(new StatLine("Stealth", TextColour.Yellow, null, 6, true, null), line);
    }

    [Fact]
    public void Skill_ShowsOnlyTheScoreInRedForAShowFour()
    {
        var line = StatLines.Skill("Immunity", 4, [10, 0, 50, 2, 1, 2], Plenty);

        Assert.Equal(new StatLine("Immunity", TextColour.Red, null, 258, false, null), line);
    }

    [Fact]
    public void Skill_ShowsThreeHundredInOrangeForASkillNotLearntYet()
    {
        var line = StatLines.Skill("Surround Hit", 2, [0, 0, 50, 2, 0, 0], Plenty);

        Assert.Equal(new StatLine("Surround Hit", TextColour.Orange, null, 300, false, null), line);
    }
}
