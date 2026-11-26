using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TheLastGate.Updates;

namespace TheLastGate.Launcher;

public partial class UpdateButtonView : Button
{
    private const int TooltipGap = 4;

    private UpdateCatalog? catalog;
    private bool checkRunning;
    private bool ctrlHeld;
    private Version? current;
    private UpdateButtonLook look = UpdateButtonLook.For(UpdateCheckState.Checking, false, false, null);
    private RollbackWindowView? rollbackWindow;
    private UpdateCheckState state = UpdateCheckState.Checking;
    private UpdateWindowView? updateWindow;
    private IUpdater? updater;

    public event Action? Restarting;

    public override void _Ready() => Pressed += OnPressed;

    // Godot keeps tooltips on screen by comparing launcher coordinates with the window's real size, which differ
    // once the launcher is scaled up to fullscreen; at the bottom of the screen the tooltip then runs off it.
    public override GodotObject _MakeCustomTooltip(string forText)
    {
        var tooltip = new Label { Text = forText, ThemeTypeVariation = "TooltipLabel" };
        Callable.From(() => PlaceAbove(tooltip)).CallDeferred();
        return tooltip;
    }

    public override void _Process(double delta)
    {
        var held = Input.IsKeyPressed(Key.Ctrl);
        if (held != ctrlHeld)
        {
            ctrlHeld = held;
            ShowLook();
        }
    }

    public void Start(IUpdater chosenUpdater, UpdateWindowView changelogWindow, RollbackWindowView earlierWindow)
    {
        updater = chosenUpdater;
        updateWindow = changelogWindow;
        rollbackWindow = earlierWindow;
        current = chosenUpdater.CurrentVersion;
        CheckAgain();
    }

    public async void CheckAgain()
    {
        if (updater is not { } chosenUpdater || !chosenUpdater.IsInstalled || current is null)
        {
            ShowState(UpdateCheckState.NotInstalled);
            return;
        }

        if (checkRunning)
        {
            return;
        }

        checkRunning = true;
        ShowState(UpdateCheckState.Checking);
        try
        {
            var releases = await Task.Run(() => chosenUpdater.ListReleases(CancellationToken.None));
            catalog = new UpdateCatalog(current, releases);
            ShowState(catalog.Newest is null ? UpdateCheckState.UpToDate : UpdateCheckState.UpdateAvailable);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Update check failed: {exception.Message}");
            ShowState(UpdateCheckState.Failed);
        }
        finally
        {
            checkRunning = false;
        }
    }

    private void PlaceAbove(Control tooltip)
    {
        var popup = tooltip.GetWindow();
        if (popup is null || popup == GetWindow())
        {
            return;
        }

        var button = GetGlobalRect();
        popup.Position = new Vector2I((int)button.Position.X, (int)button.Position.Y - popup.Size.Y - TooltipGap);
    }

    private void ShowState(UpdateCheckState newState)
    {
        state = newState;
        ShowLook();
    }

    private void ShowLook()
    {
        look = UpdateButtonLook.For(state, ctrlHeld, catalog?.RollbackChoices.Count > 0, catalog?.Newest?.Version);
        Text = look.Text;
        Disabled = !look.Enabled;
        TooltipText = look.Tooltip;
    }

    private void OnPressed()
    {
        switch (look.Action)
        {
            case UpdateButtonAction.ShowUpdate:
                OpenUpdateWindow();
                break;
            case UpdateButtonAction.ShowRollback:
                OpenRollbackWindow();
                break;
            case UpdateButtonAction.None:
                break;
            default:
                throw new UnreachableException();
        }
    }

    private void OpenUpdateWindow()
    {
        if (updater is not { } chosenUpdater || updateWindow is null || catalog?.Newest is not { } newest)
        {
            return;
        }

        updateWindow.Open(
            catalog.NewerThanCurrent,
            (progress, cancel) => Task.Run(() => chosenUpdater.Download(newest.Version, progress, cancel), cancel),
            () => Restart(newest.Version));
    }

    private void OpenRollbackWindow()
    {
        if (updater is not { } chosenUpdater || rollbackWindow is null || catalog is null || current is null)
        {
            return;
        }

        rollbackWindow.Open(
            current,
            catalog,
            (target, progress, cancel) => Task.Run(() => chosenUpdater.Download(target, progress, cancel), cancel),
            Restart);
    }

    private void Restart(Version target)
    {
        updater?.ApplyAndRestart(target);
        Restarting?.Invoke();
    }
}
