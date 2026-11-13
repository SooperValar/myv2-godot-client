using System.Text;

namespace TheLastGate.Tools.SpriteCompiler;

public sealed class CompileReport
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int BmpSpriteCount { get; set; }
    public int PngSpriteCount { get; set; }
    public long BmpDataBytes { get; set; }
    public long PngDataBytes { get; set; }
    public List<SkippedFile> Skipped { get; } = [];
    public List<int> InBothFormats { get; } = [];

    public string Format()
    {
        var text = new StringBuilder();
        if (Succeeded)
        {
            text.AppendLine("Sprite archives rebuilt.");
            text.AppendLine($"  bmplib: {BmpSpriteCount} sprites, {BmpDataBytes:N0} bytes of pixels");
            text.AppendLine($"  pnglib: {PngSpriteCount} sprites, {PngDataBytes:N0} bytes of PNGs");
        }
        else
        {
            text.AppendLine("Sprite archives NOT rebuilt. The existing archives are unchanged.");
        }

        if (Skipped.Count > 0)
        {
            text.AppendLine($"Skipped {Skipped.Count} files:");
            foreach (var skipped in Skipped)
            {
                text.AppendLine($"  {skipped.FileName}: {skipped.Reason}");
            }
        }

        if (InBothFormats.Count > 0)
        {
            text.AppendLine(
                "In both formats (the client draws the PNG): "
                + string.Join(", ", InBothFormats.Select(spriteNumber => spriteNumber.ToString("00000"))));
        }

        if (Error != null)
        {
            text.AppendLine($"Error: {Error}");
        }

        return text.ToString();
    }
}
