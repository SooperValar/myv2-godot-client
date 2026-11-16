using TheLastGate.Hud;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ExitSequenceTests
{
    private const int IdleCode = 2;
    private const int ShutdownCode = 9;
    private const int TavernCode = 10;
    private const int UserRequestCode = 12;
    private const int BannedCode = 14;

    [Fact]
    public void Click_WarnsThenRequestsTheExitThenWaits()
    {
        var exit = new ExitSequence();

        Assert.Equal(ExitStep.Warn, exit.Click());
        Assert.Equal(ExitStep.RequestExit, exit.Click());
        Assert.Equal(ExitStep.Wait, exit.Click());
    }

    [Fact]
    public void Click_LeavesFiveSecondsAfterTheRequest()
    {
        var exit = new ExitSequence();
        exit.Click();
        exit.Click();

        exit.Advance(TimeSpan.FromSeconds(4.9));
        Assert.Equal(ExitStep.Wait, exit.Click());

        exit.Advance(TimeSpan.FromSeconds(0.1));
        Assert.Equal(ExitStep.Leave, exit.Click());
    }

    [Fact]
    public void Advance_DoesNotCountBeforeTheRequest()
    {
        var exit = new ExitSequence();
        exit.Advance(TimeSpan.FromMinutes(1));

        Assert.Equal(ExitStep.Warn, exit.Click());
        Assert.Equal(ExitStep.RequestExit, exit.Click());
        Assert.Equal(ExitStep.Wait, exit.Click());
    }

    [Theory]
    [InlineData(TavernCode)]
    [InlineData(IdleCode)]
    public void ServerEnded_LeavesStraightAwayAfterAnExitRequest(int reasonCode)
    {
        var exit = new ExitSequence();
        exit.Click();
        exit.Click();

        Assert.True(exit.ServerEnded(reasonCode));
    }

    [Theory]
    [InlineData(TavernCode)]
    [InlineData(UserRequestCode)]
    public void ServerEnded_LeavesStraightAwayForACleanLogout(int reasonCode)
    {
        Assert.True(new ExitSequence().ServerEnded(reasonCode));
    }

    [Theory]
    [InlineData(IdleCode)]
    [InlineData(ShutdownCode)]
    [InlineData(BannedCode)]
    public void ServerEnded_StaysUntilTheNextClickForAProblem(int reasonCode)
    {
        var exit = new ExitSequence();

        Assert.False(exit.ServerEnded(reasonCode));
        Assert.Equal(ExitStep.Leave, exit.Click());
    }

    [Fact]
    public void WarningAndRequestLines_AreTheCurrentClientsMessages()
    {
        Assert.Equal(
            [
                " ",
                "(F12) Leaving the game without entering a tavern will cause you to take damage, and possibly lose your life. Click again if you need to leave immediately.",
                "A tavern will be located near where you entered the game.",
            ],
            ExitSequence.WarningLines);
        Assert.Equal([" ", "(F12) Exit request acknowledged. Please wait a moment..."], ExitSequence.RequestLines);
    }

    [Fact]
    public void ExitPacket_AsksTheServerToEndTheSession()
    {
        Assert.Equal(ClientPackets.OneValue(ClientCommand.Exit, 0), ExitSequence.ExitPacket);
    }
}
