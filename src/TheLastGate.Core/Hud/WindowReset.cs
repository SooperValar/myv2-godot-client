using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Hud;

public static class WindowReset
{
    public static (int Width, int Height) Size => (1280, 720);

    public static LogLine? Line(bool windowed) =>
        windowed ? new LogLine("Game window reset to default resolution.", (int)TextColour.Green) : null;
}
