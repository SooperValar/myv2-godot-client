namespace TheLastGate.Tools.SpriteCompiler;

// Pixels are RGB565, top-down and row-major. Exactly one of Pixels and RejectionReason is set.
public sealed record BmpDecodeResult(int Width, int Height, ushort[]? Pixels, string? RejectionReason)
{
    public static BmpDecodeResult Rejected(string reason) => new(0, 0, null, reason);
}
