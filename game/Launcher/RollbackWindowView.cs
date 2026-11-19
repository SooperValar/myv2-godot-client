using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Updates;

namespace TheLastGate.Launcher;

public partial class RollbackWindowView : ConfirmationDialog
{
    private const string DownloadingText = "Downloading...";
    private const string DownloadFailedText = "The download failed. Check your connection and try again.";
    private const double BytesPerMegabyte = 1_000_000;

    private static readonly Vector2 VersionButtonSize = new(0, 44);
    private readonly List<(Button Button, Version Version)> choiceButtons = [];

    private Label caption = null!;
    private UpdateCatalog? catalog;
    private Version? chosen;
    private Version? current;
    private Func<Version, Action<int>, CancellationToken, Task>? download;
    private CancellationTokenSource? downloadCancel;
    private Label error = null!;
    private Label info = null!;
    private ProgressBar progress = null!;
    private Action<Version>? restart;
    private RichTextLabel undone = null!;
    private VBoxContainer versions = null!;

    public ILauncherFontSet Fonts { get; set; } = new PixelOperatorFonts();

    public override void _Ready()
    {
        versions = GetNode<VBoxContainer>("%Versions");
        caption = GetNode<Label>("%Caption");
        undone = GetNode<RichTextLabel>("%Undone");
        info = GetNode<Label>("%Info");
        progress = GetNode<ProgressBar>("%Progress");
        error = GetNode<Label>("%Error");
        Confirmed += OnRollBackPressed;
        Canceled += () => downloadCancel?.Cancel();
    }

    public void Open(
        Version currentVersion,
        UpdateCatalog releases,
        Func<Version, Action<int>, CancellationToken, Task> downloadRelease,
        Action<Version> restartOn)
    {
        current = currentVersion;
        catalog = releases;
        download = downloadRelease;
        restart = restartOn;
        progress.Visible = false;
        error.Visible = false;
        ShowVersions();
        Choose(releases.RollbackChoices[0]);
        ShowBusy(false);
        PopupCentered();
    }

    private void ShowVersions()
    {
        foreach (var child in versions.GetChildren())
        {
            child.QueueFree();
        }

        choiceButtons.Clear();

        versions.AddChild(
            new Button
            {
                Text = $"{current} (you have this)",
                Disabled = true,
                CustomMinimumSize = VersionButtonSize,
            });

        var group = new ButtonGroup();
        foreach (var release in catalog!.RollbackChoices)
        {
            var button = new Button
            {
                Text = release.Notes.Date is { } date
                    ? $"{release.Version}\n{date:yyyy-MM-dd}"
                    : release.Version.ToString(),
                ToggleMode = true,
                ButtonGroup = group,
                CustomMinimumSize = VersionButtonSize,
            };
            button.Pressed += () => Choose(release);
            versions.AddChild(button);
            choiceButtons.Add((button, release.Version));
        }
    }

    private void Choose(AvailableRelease release)
    {
        chosen = release.Version;
        foreach (var (button, version) in choiceButtons)
        {
            button.SetPressedNoSignal(version == release.Version);
        }

        caption.Text = $"Rolling back to {release.Version} undoes these changes:";
        ChangelogText.Fill(undone, catalog!.UndoneBy(release.Version), Fonts);
        var sizeInMegabytes = Math.Round(release.FullSizeBytes / BytesPerMegabyte);
        info.Text = $"Downloads the full game ({sizeInMegabytes} MB). Your settings, characters and map notes are "
            + $"kept, and a copy of them is saved in {UserFolderBackup.BackupsFolder(OS.GetUserDataDir())} first. "
            + "The launcher restarts when it's done.";
        GetOkButton().Text = RollBackText();
    }

    private string RollBackText() => $"Roll back to {chosen}";

    private async void OnRollBackPressed()
    {
        if (chosen is not { } target || current is null || download is null || restart is null)
        {
            return;
        }

        ShowBusy(true);
        error.Visible = false;
        try
        {
            UserFolderBackup.Create(OS.GetUserDataDir(), current, DateTime.Now);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ShowError($"Couldn't back up your settings, so nothing was changed: {exception.Message}");
            return;
        }

        progress.Value = 0;
        progress.Visible = true;
        GetOkButton().Text = DownloadingText;
        using var cancel = new CancellationTokenSource();
        downloadCancel = cancel;
        try
        {
            await download(
                target,
                percent => Callable.From(() => progress.Value = percent).CallDeferred(),
                cancel.Token);
            Hide();
            restart(target);
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            progress.Visible = false;
            ShowBusy(false);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Rollback download failed: {exception.Message}");
            ShowError(DownloadFailedText);
        }
        finally
        {
            downloadCancel = null;
        }
    }

    private void ShowError(string message)
    {
        progress.Visible = false;
        error.Text = message;
        error.Visible = true;
        ShowBusy(false);
    }

    private void ShowBusy(bool busy)
    {
        GetOkButton().Disabled = busy;
        if (!busy)
        {
            GetOkButton().Text = RollBackText();
        }

        foreach (var (button, _) in choiceButtons)
        {
            button.Disabled = busy;
        }
    }
}
