using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TheLastGate.Assets;

public static class SpriteOverrides
{
    private const string PngExtension = ".png";
    private const string BmpExtension = ".bmp";
    private const string FileNumberFormat = "D5";

    // Only names the current client would open count: the number zero padded to five digits, with a .png chosen over
    // a .bmp.
    public static IReadOnlyDictionary<int, string> Find(params string[] folders)
    {
        var filesBySprite = new Dictionary<int, string>();
        foreach (var (spriteNumber, filePath) in folders.SelectMany(FindIn))
        {
            filesBySprite[spriteNumber] = filePath;
        }

        return filesBySprite;
    }

    private static Dictionary<int, string> FindIn(string folder)
    {
        var filesBySprite = new Dictionary<int, string>();
        if (!Directory.Exists(folder))
        {
            return filesBySprite;
        }

        foreach (var filePath in Directory.EnumerateFiles(folder))
        {
            var extension = Path.GetExtension(filePath);
            var isPng = extension.Equals(PngExtension, StringComparison.OrdinalIgnoreCase);
            if (!isPng && !extension.Equals(BmpExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            if (!int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var spriteNumber)
                || name != spriteNumber.ToString(FileNumberFormat, CultureInfo.InvariantCulture)
                || spriteNumber >= SpriteNumbers.MaxSprite)
            {
                continue;
            }

            if (isPng || !filesBySprite.ContainsKey(spriteNumber))
            {
                filesBySprite[spriteNumber] = filePath;
            }
        }

        return filesBySprite;
    }
}
