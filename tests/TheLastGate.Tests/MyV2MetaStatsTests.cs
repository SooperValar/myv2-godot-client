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

        Assert.Equal("Hit Point Regen", regen.Name);
        Assert.Equal("/s", regen.Affix);
        Assert.NotEqual(0, regen.Flag);
    }

    [Fact]
    public void Rows_CountEachList()
    {
        Assert.Equal(6, MyV2MetaStats.OffenseRows);
        Assert.Equal(7, MyV2MetaStats.DefenseRows);
    }
}
