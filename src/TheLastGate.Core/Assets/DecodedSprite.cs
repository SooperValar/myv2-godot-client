namespace TheLastGate.Assets;

// Pixels are RGBA8, rows tightly packed, with the magenta colour key already turned into alpha 0.
public sealed record DecodedSprite(int Width, int Height, byte[] Rgba);
