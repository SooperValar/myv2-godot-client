using Godot;

namespace TheLastGate.World;

// The server's item glow: 1 Magic, 2 Rare, 3 Epic, 4 Legendary, 5 a grave or body still holding items. The colours and
// strengths are the ones merc.exe draws.
public static class TierOutline
{
    public const int None = 0;
    public const int HoldsItems = 5;

    private const int ObjectGlowBits = 7;
    private const int CharacterHoldsItemsBit = 8;

    private static readonly Color[] Colours =
    [
        new(0, 0, 0, 0),
        Color.Color8(40, 120, 255, 150),
        Color.Color8(255, 215, 0, 150),
        Color.Color8(175, 60, 255, 160),
        Color.Color8(255, 120, 0, 170),
        Color.Color8(210, 210, 210, 90),
    ];

    public static Color For(int glow) => glow is > None and <= HoldsItems ? Colours[glow] : Colours[None];

    public static int OfObject(int tileGlow) => tileGlow & ObjectGlowBits;

    public static int OfCharacter(int tileGlow) => (tileGlow & CharacterHoldsItemsBit) != 0 ? HoldsItems : None;
}
