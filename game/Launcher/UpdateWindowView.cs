using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Updates;

namespace TheLastGate.Launcher;

public partial class UpdateWindowView : ConfirmationDialog
{
    private const string UpdateText = "Update";
    private const string DownloadingText = "Downloading...";
    private const string DownloadFailedText = "The download failed. Check your connection and try again.";

    private RichTextLabel changelog = null!;
    private Func<Action<int>, CancellationToken, Task>? download;
    private CancellationTokenSource? downloadCancel;
    private Label error = null!;
    private ProgressBar progress = null!;
    private Action? restart;

    public ILauncherFontSet Fonts { get; set; } = new PixelOperatorFonts();

    public override void _Ready()
    {
        changelog = GetNode<RichTextLabel>("%Changelog");
        progress = GetNode<ProgressBar>("%Progress");
        error = GetNode<Label>("%Error");
        error.Text = DownloadFailedText;
        Confirmed += OnUpdatePressed;
        Canceled += () => downloadCancel?.Cancel();
    }

    public void Open(
        IReadOnlyList<AvailableRelease> newer,
        Func<Action<int>, CancellationToken, Task> download,
        Action restart)
    {
        this.download = download;
        this.restart = restart;
        ChangelogText.Fill(changelog, newer, Fonts);
        progress.Visible = false;
        error.Visible = false;
        ShowBusy(false);
        PopupCentered();
    }

    private async void OnUpdatePressed()
    {
        if (download is null || restart is null)
        {
            return;
        }

        ShowBusy(true);
        error.Visible = false;
        progress.Value = 0;
        progress.Visible = true;
        using var cancel = new CancellationTokenSource();
        downloadCancel = cancel;
        try
        {
            await download(percent => Callable.From(() => progress.Value = percent).CallDeferred(), cancel.Token);
            Hide();
            restart();
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            progress.Visible = false;
            ShowBusy(false);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Update download failed: {exception.Message}");
            progress.Visible = false;
            error.Visible = true;
            ShowBusy(false);
        }
        finally
        {
            downloadCancel = null;
        }
    }

    private void ShowBusy(bool busy)
    {
        GetOkButton().Disabled = busy;
        GetOkButton().Text = busy ? DownloadingText : UpdateText;
    }
}
