using TheLastGate.State;

namespace TheLastGate.Tests;

public class MyV2MetaStatsTests
{
    [Fact]
    public void Terminology_NamesHitAndParryScoreWhereTheHudReadsThem()
    {
        var stats = new Terminology().MetaStats;

        Assert.Equal("Hit Score", stats[5].Name);
        Assert.Equal("Parry Score", stats[6].Name);
    }

    [Fact]
    public void Fill_GivesRegenInHundredthsPerSecond()
    {
        var regen = new Terminology().MetaStats[50];

        Assert.Equal("Health Regen Rate", regen.Name);
        Assert.Equal("/s", regen.Affix);
        Assert.NotEqual(0, regen.Flag);
    }

    [Fact]
    public void Slots_ListEachListInOrder()
    {
        Assert.Equal(23, MyV2MetaStats.OffenseSlots.Length);
        Assert.Equal(7, MyV2MetaStats.OffenseSlots[0]);
        Assert.Equal([48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58], MyV2MetaStats.DefenseSlots);
    }

    [Fact]
    public void Fill_NamesTheThreeSpeeds()
    {
        var stats = new Terminology().MetaStats;

        Assert.Equal("Attack Speed", stats[3].Name);
        Assert.Equal("Walk Speed", stats[57].Name);
        Assert.Equal("Speed", stats[58].Name);
        Assert.Equal("/s", stats[57].Affix);
    }

    [Fact]
    public void Fill_GivesHeadingsNoNumber()
    {
        var heading = new Terminology().MetaStats[7];

        Assert.Equal("  Passive Stats:", heading.Name);
        Assert.Equal("", MetaStat.ValueText(65535, heading.Flag != 0));
    }
}
