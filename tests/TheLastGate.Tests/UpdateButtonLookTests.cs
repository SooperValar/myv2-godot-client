using TheLastGate.Updates;

namespace TheLastGate.Tests;

public class UpdateButtonLookTests
{
    private static readonly Version Newest = new(1, 2, 0);

    private static UpdateButtonLook Look(
        UpdateCheckState state,
        bool ctrlHeld = false,
        bool hasRollbackChoices = true) =>
        UpdateButtonLook.For(
            state,
            ctrlHeld,
            hasRollbackChoices,
            state == UpdateCheckState.UpdateAvailable ? Newest : null);

    [Fact]
    public void For_CheckingIsGreyedOut()
    {
        Assert.Equal(
            new UpdateButtonLook("Checking...", false, "Checking for updates.", UpdateButtonAction.None),
            Look(UpdateCheckState.Checking));
    }

    [Fact]
    public void For_NotInstalledIsGreyedOut()
    {
        Assert.Equal(
            new UpdateButtonLook(
                "Update",
                false,
                "Updates only work in a copy installed with Setup.",
                UpdateButtonAction.None),
            Look(UpdateCheckState.NotInstalled));
    }

    [Fact]
    public void For_FailedCheckIsGreyedOut()
    {
        Assert.Equal(
            new UpdateButtonLook(
                "Update",
                false,
                "Couldn't check for updates. Reopen the launcher to try again.",
                UpdateButtonAction.None),
            Look(UpdateCheckState.Failed));
    }

    [Fact]
    public void For_UpToDateIsGreyedOut()
    {
        Assert.Equal(
            new UpdateButtonLook("Update", false, "You have the newest version.", UpdateButtonAction.None),
            Look(UpdateCheckState.UpToDate));
    }

    [Fact]
    public void For_UpdateAvailableOpensTheChangelog()
    {
        Assert.Equal(
            new UpdateButtonLook("Update", true, "Version 1.2.0 is available.", UpdateButtonAction.ShowUpdate),
            Look(UpdateCheckState.UpdateAvailable));
    }

    [Theory]
    [InlineData(UpdateCheckState.Checking)]
    [InlineData(UpdateCheckState.NotInstalled)]
    [InlineData(UpdateCheckState.Failed)]
    [InlineData(UpdateCheckState.UpToDate)]
    [InlineData(UpdateCheckState.UpdateAvailable)]
    public void For_CtrlWithChoicesOpensRollback(UpdateCheckState state)
    {
        Assert.Equal(
            new UpdateButtonLook("Rollback", true, "Go back to an earlier version.", UpdateButtonAction.ShowRollback),
            Look(state, true));
    }

    [Theory]
    [InlineData(UpdateCheckState.Checking)]
    [InlineData(UpdateCheckState.UpToDate)]
    public void For_CtrlWithoutChoicesIsGreyedOut(UpdateCheckState state)
    {
        Assert.Equal(
            new UpdateButtonLook(
                "Rollback",
                false,
                "There's no earlier version to roll back to.",
                UpdateButtonAction.None),
            Look(state, true, false));
    }

    [Fact]
    public void For_CtrlWinsOverAnAvailableUpdate()
    {
        Assert.Equal(UpdateButtonAction.ShowRollback, Look(UpdateCheckState.UpdateAvailable, true).Action);
    }
}
