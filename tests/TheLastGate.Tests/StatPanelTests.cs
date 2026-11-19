using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class StatPanelTests
{
    [Fact]
    public void LastMetaRow_LeavesTenOfEachListsFortyOneStatsInView()
    {
        Assert.Equal(31, StatPanel.LastMetaRow);
    }

    [Fact]
    public void Choose_ListSkillsAgain_AddsTheMetaStatsOnTop()
    {
        var panel = new StatPanel();
        panel.Choose(StatListMode.Skills);

        Assert.Equal(StatListMode.SkillsAndMeta, panel.Mode);
    }

    [Fact]
    public void Choose_ListSkillsFromSkillsAndMeta_GoesBackToSkillsAlone()
    {
        var panel = new StatPanel();
        panel.Choose(StatListMode.Skills);
        panel.Choose(StatListMode.Skills);

        Assert.Equal(StatListMode.Skills, panel.Mode);
    }

    [Fact]
    public void Choose_ListSkillsFromOffenses_ShowsSkillsAlone()
    {
        var panel = new StatPanel();
        panel.Choose(StatListMode.Offenses);
        panel.Choose(StatListMode.Skills);

        Assert.Equal(StatListMode.Skills, panel.Mode);
    }

    [Theory]
    [InlineData(StatListMode.Skills, StatListMode.Skills, true)]
    [InlineData(StatListMode.SkillsAndMeta, StatListMode.Skills, true)]
    [InlineData(StatListMode.Offenses, StatListMode.Skills, false)]
    [InlineData(StatListMode.Offenses, StatListMode.Offenses, true)]
    [InlineData(StatListMode.Defenses, StatListMode.Offenses, false)]
    [InlineData(StatListMode.Defenses, StatListMode.Defenses, true)]
    public void IsMarked_MarksTheButtonForTheList(StatListMode mode, StatListMode button, bool marked)
    {
        var panel = PanelIn(mode);

        Assert.Equal(marked, panel.IsMarked(button));
    }

    [Theory]
    [InlineData(StatListMode.Skills, false, false)]
    [InlineData(StatListMode.SkillsAndMeta, false, true)]
    [InlineData(StatListMode.Offenses, true, true)]
    [InlineData(StatListMode.Defenses, true, true)]
    public void Modes_ShowMetaStatsWhereTheSdlClientDoes(StatListMode mode, bool metaBelow, bool metaOnTop)
    {
        var panel = PanelIn(mode);

        Assert.Equal(metaBelow, panel.ShowsMetaStats);
        Assert.Equal(metaOnTop, panel.ShowsMetaOnTop);
    }

    [Fact]
    public void ScrollTo_InOffenses_MovesTheMetaListAndKeepsTheSkillRow()
    {
        var panel = new StatPanel();
        panel.ScrollTo(5);
        panel.Choose(StatListMode.Offenses);
        panel.ScrollTo(50);

        Assert.Equal(31, panel.Row);
        Assert.Equal(31, panel.LastRow);
        Assert.Equal(5, panel.Skills.Row);
    }

    [Fact]
    public void Row_BackInSkills_IsTheSkillRow()
    {
        var panel = new StatPanel();
        panel.ScrollTo(5);
        panel.Choose(StatListMode.Defenses);
        panel.ScrollBy(3);
        panel.Choose(StatListMode.Skills);

        Assert.Equal(5, panel.Row);
        Assert.Equal(45, panel.LastRow);
    }

    [Fact]
    public void ScrollBy_StaysAtTheTop()
    {
        var panel = PanelIn(StatListMode.Offenses);
        panel.ScrollBy(-1);

        Assert.Equal(0, panel.MetaRow);
    }

    [Fact]
    public void TopMeta_InSkills_IsNone()
    {
        Assert.Null(new StatPanel().TopMeta(0));
    }

    [Theory]
    [InlineData(StatListMode.SkillsAndMeta)]
    [InlineData(StatListMode.Offenses)]
    [InlineData(StatListMode.Defenses)]
    public void TopMeta_ShowsTheFirstSevenStatsUnscrolled(StatListMode mode)
    {
        var panel = PanelIn(mode);
        panel.ScrollBy(4);

        Assert.Equal(6, panel.TopMeta(6));
    }

    [Fact]
    public void BottomMeta_InOffenses_StartsAfterTheTopRows()
    {
        var panel = PanelIn(StatListMode.Offenses);
        panel.ScrollTo(2);

        Assert.Equal(10, panel.BottomMeta(1));
    }

    [Fact]
    public void BottomMeta_InDefenses_StartsAtTheDefenseStats()
    {
        var panel = PanelIn(StatListMode.Defenses);

        Assert.Equal(48, panel.BottomMeta(0));
    }

    [Fact]
    public void BottomMeta_ScrolledToTheEnd_ReachesTheLastStatOfEachList()
    {
        var offenses = PanelIn(StatListMode.Offenses);
        offenses.ScrollTo(StatPanel.LastMetaRow);
        var defenses = PanelIn(StatListMode.Defenses);
        defenses.ScrollTo(StatPanel.LastMetaRow);

        Assert.Equal(47, offenses.BottomMeta(9));
        Assert.Equal(88, defenses.BottomMeta(9));
    }

    [Theory]
    [InlineData(StatListMode.Skills)]
    [InlineData(StatListMode.SkillsAndMeta)]
    public void BottomMeta_WithSkillsBelow_IsNone(StatListMode mode)
    {
        Assert.Null(PanelIn(mode).BottomMeta(0));
    }

    [Theory]
    [InlineData(StatListMode.Skills, "Makes the left panel display skill scores and experience points (default).")]
    [InlineData(
        StatListMode.Offenses,
        "Makes the left panel display Offense-related statistics, such as attack damage and critical hit rate.")]
    [InlineData(
        StatListMode.Defenses,
        "Makes the left panel display Defense-related statistics, such as eHP and regeneration rates.")]
    public void Help_MatchesTheSdlClient(StatListMode button, string help)
    {
        Assert.Equal(help, StatPanel.Help(button));
    }

    private static StatPanel PanelIn(StatListMode mode)
    {
        var panel = new StatPanel();
        if (mode == StatListMode.SkillsAndMeta)
        {
            panel.Choose(StatListMode.Skills);
        }
        else if (mode != StatListMode.Skills)
        {
            panel.Choose(mode);
        }

        return panel;
    }
}
