namespace TheLastGate.Windows;

// For a short while after a window is closed, any window the server opens is closed again straight away. The current
// client shares this pause between shops, waypoints, the skill tree and the text windows.
public sealed class WindowPause
{
    public const int Ticks = 24;

    private int remaining;

    public bool Active => remaining > 0;

    public void Start() => remaining = Ticks;

    public void Cancel() => remaining = 0;

    public void EndOfTick()
    {
        if (remaining > 0)
        {
            remaining--;
        }
    }
}
