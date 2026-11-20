using System;
using System.Diagnostics;

namespace TheLastGate.Updates;

public sealed record UpdateButtonLook(string Text, bool Enabled, string Tooltip, UpdateButtonAction Action)
{
    private const string UpdateText = "Update";

    private static readonly UpdateButtonLook RollbackLook = new(
        "Rollback",
        true,
        "Go back to an earlier version.",
        UpdateButtonAction.ShowRollback);

    private static readonly UpdateButtonLook NoRollbackLook = new(
        "Rollback",
        false,
        "There's no earlier version to roll back to.",
        UpdateButtonAction.None);

    public static UpdateButtonLook For(UpdateCheckState state, bool ctrlHeld, bool hasRollbackChoices, Version? newest)
    {
        if (ctrlHeld)
        {
            return hasRollbackChoices ? RollbackLook : NoRollbackLook;
        }

        return state switch
        {
            UpdateCheckState.Checking => Greyed("Checking...", "Checking for updates."),
            UpdateCheckState.NotInstalled => Greyed(UpdateText, "Updates only work in a copy installed with Setup."),
            UpdateCheckState.Failed => Greyed(
                UpdateText,
                "Couldn't check for updates. Reopen the launcher to try again."),
            UpdateCheckState.UpToDate => Greyed(UpdateText, "You have the newest version."),
            UpdateCheckState.UpdateAvailable => new UpdateButtonLook(
                UpdateText,
                true,
                $"Version {newest} is available.",
                UpdateButtonAction.ShowUpdate),
            _ => throw new UnreachableException(),
        };
    }

    private static UpdateButtonLook Greyed(string text, string tooltip) =>
        new(text, false, tooltip, UpdateButtonAction.None);
}
