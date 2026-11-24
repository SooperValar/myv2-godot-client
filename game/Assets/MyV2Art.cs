using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TheLastGate.Assets;

// The MyV2 client keeps its art as gx00 (the bmplib format), pnglib, and loose NNNNN.png / NNNNN.bmp files in GFX,
// and looks a sprite up in the order: loose PNG, pnglib, loose BMP, gx00.
public sealed class MyV2Art : IDisposable
{
    private const string BaseIndexFile = "gx00.idx";
    private const string BaseDataFile = "gx00.dat";
    private const string PngIndexFile = "pnglib.idx";
    private const string PngDataFile = "pnglib.dat";
    private const string PngExtension = ".png";
    private const string BmpExtension = ".bmp";
    private const string FileNumberFormat = "D5";

    private static readonly string[] GraphicsFolderNames = ["GFX", "gfx"];

    private readonly IReadOnlyDictionary<int, string> looseBmps;
    private readonly IReadOnlyDictionary<int, string> loosePngs;

    private MyV2Art(string folder, BmpLibrary baseLibrary, PngLibrary pngLibrary)
    {
        Folder = folder;
        BaseLibrary = baseLibrary;
        PngLibrary = pngLibrary;
        var graphicsFolder = FindGraphicsFolder(folder);
        loosePngs = LooseFiles(graphicsFolder, PngExtension);
        looseBmps = LooseFiles(graphicsFolder, BmpExtension);
    }

    public string Folder { get; }

    public BmpLibrary BaseLibrary { get; }

    public PngLibrary PngLibrary { get; }

    public static MyV2Art? Open(string? folder)
    {
        if (folder == null || !File.Exists(Path.Combine(folder, BaseIndexFile)))
        {
            return null;
        }

        var baseLibrary = new BmpLibrary(Path.Combine(folder, BaseIndexFile), Path.Combine(folder, BaseDataFile));
        try
        {
            var pngLibrary = new PngLibrary(Path.Combine(folder, PngIndexFile), Path.Combine(folder, PngDataFile));
            return new MyV2Art(folder, baseLibrary, pngLibrary);
        }
        catch
        {
            baseLibrary.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        PngLibrary.Dispose();
        BaseLibrary.Dispose();
    }

    public SpriteSource FindSource(int spriteNumber)
    {
        if (loosePngs.ContainsKey(spriteNumber))
        {
            return SpriteSource.MyV2Loose;
        }

        if (PngLibrary.Contains(spriteNumber))
        {
            return SpriteSource.MyV2PngLibrary;
        }

        if (looseBmps.ContainsKey(spriteNumber))
        {
            return SpriteSource.MyV2Loose;
        }

        return BaseLibrary.Contains(spriteNumber) ? SpriteSource.MyV2Base : SpriteSource.None;
    }

    public string LooseFile(int spriteNumber) =>
        loosePngs.TryGetValue(spriteNumber, out var pngPath) ? pngPath : looseBmps[spriteNumber];

    private static string? FindGraphicsFolder(string folder)
    {
        foreach (var name in GraphicsFolderNames)
        {
            var path = Path.Combine(folder, name);
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static Dictionary<int, string> LooseFiles(string? graphicsFolder, string extension)
    {
        var filesBySprite = new Dictionary<int, string>();
        if (graphicsFolder == null)
        {
            return filesBySprite;
        }

        foreach (var filePath in Directory.EnumerateFiles(graphicsFolder))
        {
            if (!Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            if (int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var spriteNumber)
                && name == spriteNumber.ToString(FileNumberFormat, CultureInfo.InvariantCulture)
                && spriteNumber < SpriteNumbers.ClientArtBase)
            {
                filesBySprite[spriteNumber] = filePath;
            }
        }

        return filesBySprite;
    }
}
