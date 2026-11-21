using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class ExitSequenceTests
{
    private const int IdleCode = 2;
    private const int ShutdownCode = 9;
    private const int TavernCode = 10;
    private const int UserRequestCode = 12;
    private const int BannedCode = 14;

    [TestCase]
    public void Click_WarnsThenRequestsTheExitThenWaits()
    {
        var exit = new ExitSequence();

        AssertThat(exit.Click()).IsEqual(ExitStep.Warn);
        AssertThat(exit.Click()).IsEqual(ExitStep.RequestExit);
        AssertThat(exit.Click()).IsEqual(ExitStep.Wait);
    }

    [TestCase]
    public void Click_LeavesFiveSecondsAfterTheRequest()
    {
        var exit = new ExitSequence();
        exit.Click();
        exit.Click();

        exit.Advance(TimeSpan.FromSeconds(4.9));
        AssertThat(exit.Click()).IsEqual(ExitStep.Wait);

        exit.Advance(TimeSpan.FromSeconds(0.1));
        AssertThat(exit.Click()).IsEqual(ExitStep.Leave);
    }

    [TestCase]
    public void Advance_DoesNotCountBeforeTheRequest()
    {
        var exit = new ExitSequence();
        exit.Advance(TimeSpan.FromMinutes(1));

        AssertThat(exit.Click()).IsEqual(ExitStep.Warn);
        AssertThat(exit.Click()).IsEqual(ExitStep.RequestExit);
        AssertThat(exit.Click()).IsEqual(ExitStep.Wait);
    }

    [TestCase(TavernCode)]
    [TestCase(IdleCode)]
    public void ServerEnded_LeavesStraightAwayAfterAnExitRequest(int reasonCode)
    {
        var exit = new ExitSequence();
        exit.Click();
        exit.Click();

        AssertThat(exit.ServerEnded(reasonCode)).IsTrue();
    }

    [TestCase(TavernCode)]
    [TestCase(UserRequestCode)]
    public void ServerEnded_LeavesStraightAwayForACleanLogout(int reasonCode)
    {
        AssertThat(new ExitSequence().ServerEnded(reasonCode)).IsTrue();
    }

    [TestCase(IdleCode)]
    [TestCase(ShutdownCode)]
    [TestCase(BannedCode)]
    public void ServerEnded_StaysUntilTheNextClickForAProblem(int reasonCode)
    {
        var exit = new ExitSequence();

        AssertThat(exit.ServerEnded(reasonCode)).IsFalse();
        AssertThat(exit.Click()).IsEqual(ExitStep.Leave);
    }

    [TestCase]
    public void WarningAndRequestLines_AreTheCurrentClientsMessages()
    {
        AssertThat(ExitSequence.WarningLines)
            .ContainsExactly(
                " ",
                "(F12) Leaving the game without entering a tavern will cause you to take damage, and possibly lose your life. Click again if you need to leave immediately.",
                "A tavern will be located near where you entered the game.");
        AssertThat(ExitSequence.RequestLines)
            .ContainsExactly(" ", "(F12) Exit request acknowledged. Please wait a moment...");
    }
}
