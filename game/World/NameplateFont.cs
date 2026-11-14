using Godot;

namespace TheLastGate.World;

// What a nameplate's text is drawn with. The current client's fonts carry their colour in the font itself, so they use
// white; any other Godot font can be swapped in with its own size and colour, and is centred by its measured width.
public readonly record struct NameplateFont(Font Font, int Size, Color Colour, bool CentresLikeTheCurrentClient);
