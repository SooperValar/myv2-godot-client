using TheLastGate.Assets;

namespace TheLastGate.Tools.SpriteCompiler;

public static class SpriteSourceScanner
{
    private const int MaxDigits = 6;

    public static SpriteSourceSet Scan(string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new SpriteCompileException($"The source sprite folder \"{folder}\" doesn't exist.");
        }

        var bitmaps = new SortedDictionary<int, string>();
        var pngs = new SortedDictionary<int, string>();
        var skipped = new List<SkippedFile>();

        foreach (var filePath in Directory.EnumerateFiles(folder).OrderBy(path => path, StringComparer.Ordinal))
        {
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var stem = Path.GetFileNameWithoutExtension(fileName);

            var isSpriteName = (extension == ".bmp" || extension == ".png")
                && stem.Length is > 0 and <= MaxDigits
                && stem.All(character => character is >= '0' and <= '9');
            if (!isSpriteName)
            {
                skipped.Add(new SkippedFile(fileName, "not a sprite file name (NNNNN.bmp or NNNNN.png)"));
                continue;
            }

            var spriteNumber = int.Parse(stem);
            var isBitmap = extension == ".bmp";
            var limit = isBitmap ? SpriteNumbers.BmpSlotCount : SpriteNumbers.MaxSprite;
            if (spriteNumber >= limit)
            {
                skipped.Add(new SkippedFile(fileName, $"sprite number out of range (must be below {limit})"));
                continue;
            }

            var target = isBitmap ? bitmaps : pngs;
            if (target.TryGetValue(spriteNumber, out var existing))
            {
                throw new SpriteCompileException(
                    $"\"{Path.GetFileName(existing)}\" and \"{fileName}\" are both sprite {spriteNumber}. Remove one.");
            }

            target[spriteNumber] = filePath;
        }

        var inBothFormats = bitmaps.Keys.Where(pngs.ContainsKey).ToList();
        return new SpriteSourceSet(bitmaps, pngs, skipped, inBothFormats);
    }
}
