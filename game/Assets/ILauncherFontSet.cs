using Godot;

namespace TheLastGate.Assets;

// The launcher's fonts, swapped separately from the game's font set.
public interface ILauncherFontSet
{
    Font Regular { get; }

    // For buttons.
    Font Bold { get; }

    int Size { get; }
}
