using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
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

    [TestCase]
    public void For_CheckingIsGreyedOut() =>
        AssertThat(Look(UpdateCheckState.Checking))
            .IsEqual(new UpdateButtonLook("Checking...", false, "Checking for updates.", UpdateButtonAction.None));

    [TestCase]
    public void For_NotInstalledIsGreyedOut() =>
        AssertThat(Look(UpdateCheckState.NotInstalled))
            .IsEqual(
                new UpdateButtonLook(
                    "Update",
                    false,
                    "Updates only work in a copy installed with Setup.",
                    UpdateButtonAction.None));

    [TestCase]
    public void For_FailedCheckIsGreyedOut() =>
        AssertThat(Look(UpdateCheckState.Failed))
            .IsEqual(
                new UpdateButtonLook(
                    "Update",
                    false,
                    "Couldn't check for updates. Reopen the launcher to try again.",
                    UpdateButtonAction.None));

    [TestCase]
    public void For_UpToDateIsGreyedOut() =>
        AssertThat(Look(UpdateCheckState.UpToDate))
            .IsEqual(new UpdateButtonLook("Update", false, "You have the newest version.", UpdateButtonAction.None));

    [TestCase]
    public void For_UpdateAvailableOpensTheChangelog() =>
        AssertThat(Look(UpdateCheckState.UpdateAvailable))
            .IsEqual(
                new UpdateButtonLook("Update", true, "Version 1.2.0 is available.", UpdateButtonAction.ShowUpdate));

    [TestCase(UpdateCheckState.Checking)]
    [TestCase(UpdateCheckState.NotInstalled)]
    [TestCase(UpdateCheckState.Failed)]
    [TestCase(UpdateCheckState.UpToDate)]
    [TestCase(UpdateCheckState.UpdateAvailable)]
    public void For_CtrlWithChoicesOpensRollback(UpdateCheckState state) =>
        AssertThat(Look(state, true))
            .IsEqual(
                new UpdateButtonLook(
                    "Rollback",
                    true,
                    "Go back to an earlier version.",
                    UpdateButtonAction.ShowRollback));

    [TestCase(UpdateCheckState.Checking)]
    [TestCase(UpdateCheckState.UpToDate)]
    public void For_CtrlWithoutChoicesIsGreyedOut(UpdateCheckState state) =>
        AssertThat(Look(state, true, false))
            .IsEqual(
                new UpdateButtonLook(
                    "Rollback",
                    false,
                    "There's no earlier version to roll back to.",
                    UpdateButtonAction.None));

    [TestCase]
    public void For_CtrlWinsOverAnAvailableUpdate() =>
        AssertThat(Look(UpdateCheckState.UpdateAvailable, true).Action).IsEqual(UpdateButtonAction.ShowRollback);
}
