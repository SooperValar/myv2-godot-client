using TheLastGate.Net;
using TheLastGate.Planner;
using TheLastGate.State;
using TheLastGate.Stats;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class PlannerWindowTests
{
    private readonly PlannerWindow window = new();

    [Fact]
    public void Toggle_OpensAndCloses()
    {
        window.Toggle();
        Assert.True(window.IsOpen);

        window.Toggle();
        Assert.False(window.IsOpen);
    }

    [Fact]
    public void TheFirstOpen_AsksForTheMetaStats()
    {
        Assert.True(window.Toggle());
        window.Toggle();

        Assert.False(window.Toggle());
    }

    [Fact]
    public void Close_ShutsTheAdvancedWindowToo()
    {
        window.Toggle();
        window.AdvancedOpen = true;

        Assert.True(window.Close());

        Assert.False(window.IsOpen);
        Assert.False(window.AdvancedOpen);
        Assert.False(window.Close());
    }

    [Fact]
    public void Escape_ClosesThePlannerWithTheOtherWindows()
    {
        var state = new GameState();
        state.PlannerWindow.Toggle();

        Assert.True(state.CloseWindowsByEscape());

        Assert.False(state.PlannerWindow.IsOpen);
    }

    [Fact]
    public void ToggleFocus_FocusesAndClearsTheSameTarget()
    {
        window.ToggleFocus(PlannerFocus.OnSkill(3));
        Assert.Equal(PlannerFocus.OnSkill(3), window.Focus);

        window.ToggleFocus(PlannerFocus.OnSkill(3));
        Assert.Null(window.Focus);
    }

    [Fact]
    public void ToggleFocus_MovesBetweenSkillsAndCombatStats()
    {
        window.ToggleFocus(PlannerFocus.OnSkill(3));

        window.ToggleFocus(PlannerFocus.OnStat(CombatStat.Armor));

        Assert.Equal(PlannerFocus.OnStat(CombatStat.Armor), window.Focus);
    }

    [Fact]
    public void ClearFocus_ForgetsTheFocus()
    {
        window.ToggleFocus(PlannerFocus.OnStat(CombatStat.Hit));

        window.ClearFocus();

        Assert.Null(window.Focus);
    }

    [Fact]
    public void TheHeader_SaysWhatIsLeftAndWhatIsPlanned()
    {
        Assert.Equal("Exp left 1200    Planned 300", PlannerWindow.Header(1200, 300));
    }

    [Fact]
    public void TheSkillPlannerOption_IsTheCurrentClientsAndStartsOff()
    {
        var rows = GeneralOptions.Rows.Select(row => row.Option).ToList();
        var row = GeneralOptions.Rows.Single(row => row.Option == GeneralOption.SkillPlanner);

        Assert.Equal("Enable the skill planner: the Planner button below the skill list and its hotkey.", row.Label);
        Assert.False(row.Default);
        Assert.False(row.GodotOnly);
        Assert.Equal(rows.IndexOf(GeneralOption.ExpandedQuickStats) + 1, rows.IndexOf(GeneralOption.SkillPlanner));
    }

    [Fact]
    public void Reset_ForgetsThePlan()
    {
        var plan = new StatPlan();
        int[] values = [10, 0, 100, 1, 0, 0];
        plan.Raise(0, values, 100000, 3);

        plan.Reset();

        Assert.Equal(0, plan.Raised(0));
        Assert.Equal(0, plan.PointsUsed);
        Assert.Empty(plan.Commit());
    }

    [Fact]
    public void TheMetaRequest_IsTheMetaCommandWithNoValue()
    {
        var packet = PlannerWindow.MetaRequest();

        Assert.Equal(Protocol.PacketSize, packet.Length);
        Assert.Equal((byte)ClientCommand.Meta, packet[0]);
        Assert.All(packet.Skip(1), value => Assert.Equal(0, value));
    }
}
