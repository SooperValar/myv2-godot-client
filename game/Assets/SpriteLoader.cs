using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Godot;

namespace TheLastGate.Assets;

public sealed class SpriteLoader : IDisposable
{
    private readonly BmpLibrary bmpLibrary;
    private readonly SpriteCache<Texture2D> cache;
    private readonly IReadOnlyDictionary<int, string> overrides;
    private readonly PngLibrary pngLibrary;
    private readonly string uiFolder;
    private readonly Dictionary<int, Texture2D?> uiPictures = [];

    public SpriteLoader(string dataPath, long cacheBudgetBytes)
    {
        overrides = SpriteOverrides.Find(Path.Combine(dataPath, DataLayout.Overrides));
        uiFolder = Path.Combine(dataPath, DataLayout.Ui);
        var archiveFolder = Path.Combine(dataPath, DataLayout.Archives);
        pngLibrary = new PngLibrary(
            Path.Combine(archiveFolder, DataLayout.PngIndexFile),
            Path.Combine(archiveFolder, DataLayout.PngDataFile));
        try
        {
            bmpLibrary = new BmpLibrary(
                Path.Combine(archiveFolder, DataLayout.BmpIndexFile),
                Path.Combine(archiveFolder, DataLayout.BmpDataFile));
        }
        catch
        {
            pngLibrary.Dispose();
            throw;
        }

        cache = new SpriteCache<Texture2D>(cacheBudgetBytes, texture => texture.Dispose());
    }

    public void Dispose()
    {
        cache.Clear();
        foreach (var picture in uiPictures.Values)
        {
            picture?.Dispose();
        }

        uiPictures.Clear();
        pngLibrary.Dispose();
        bmpLibrary.Dispose();
    }

    // A loose override comes first, then pnglib, then bmplib, as in the current client.
    public SpriteSource FindSource(int spriteNumber)
    {
        if (overrides.ContainsKey(spriteNumber))
        {
            return SpriteSource.Override;
        }

        if (pngLibrary.Contains(spriteNumber))
        {
            return SpriteSource.PngLibrary;
        }

        return bmpLibrary.Contains(spriteNumber) ? SpriteSource.BmpLibrary : SpriteSource.None;
    }

    public Texture2D? GetTexture(int spriteNumber)
    {
        if (cache.TryGet(spriteNumber, out var cachedTexture))
        {
            return cachedTexture;
        }

        var image = LoadImage(spriteNumber);
        if (image == null)
        {
            return null;
        }

        var texture = ImageTexture.CreateFromImage(image);
        cache.Add(spriteNumber, texture, (long)image.GetWidth() * image.GetHeight() * SpritePixels.Rgba8BytesPerPixel);
        return texture;
    }

    public Texture2D? GetUiPicture(int pictureNumber)
    {
        if (uiPictures.TryGetValue(pictureNumber, out var known))
        {
            return known;
        }

        var path = Path.Combine(uiFolder, pictureNumber.ToString("00000", CultureInfo.InvariantCulture) + ".png");
        var image = new Image();
        Texture2D? picture = null;
        if (File.Exists(path) && image.LoadPngFromBuffer(File.ReadAllBytes(path)) == Error.Ok)
        {
            picture = ImageTexture.CreateFromImage(image);
        }
        else
        {
            GD.PushWarning($"UI picture {pictureNumber} couldn't be loaded from {path}");
        }

        uiPictures[pictureNumber] = picture;
        return picture;
    }

    private Image? LoadImage(int spriteNumber)
    {
        try
        {
            return FindSource(spriteNumber) switch
            {
                SpriteSource.Override => DecodeOverride(spriteNumber, overrides[spriteNumber]),
                SpriteSource.PngLibrary => DecodePng(spriteNumber, pngLibrary.ReadPng(spriteNumber)!),
                SpriteSource.BmpLibrary => ToImage(bmpLibrary.Read(spriteNumber)!),
                _ => null,
            };
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
            GD.PushError($"Sprite {spriteNumber} couldn't be read: {exception.Message}");
            return null;
        }
    }

    private static Image? DecodeOverride(int spriteNumber, string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        return Path.GetExtension(filePath).Equals(".png", StringComparison.OrdinalIgnoreCase)
            ? DecodePng(spriteNumber, fileBytes)
            : Decode(spriteNumber, image => image.LoadBmpFromBuffer(fileBytes));
    }

    private static Image? DecodePng(int spriteNumber, byte[] pngBytes) =>
        Decode(spriteNumber, image => image.LoadPngFromBuffer(pngBytes));

    private static Image? Decode(int spriteNumber, Func<Image, Error> load)
    {
        var image = new Image();
        var result = load(image);
        if (result != Error.Ok)
        {
            GD.PushWarning($"Sprite {spriteNumber} couldn't be decoded: {result}");
            return null;
        }

        image.Convert(Image.Format.Rgba8);
        var pixels = image.GetData();
        SpritePixels.ApplyMagentaKey(pixels);
        return Image.CreateFromData(image.GetWidth(), image.GetHeight(), false, Image.Format.Rgba8, pixels);
    }

    private static Image ToImage(DecodedSprite sprite) =>
        Image.CreateFromData(sprite.Width, sprite.Height, false, Image.Format.Rgba8, sprite.Rgba);
}
