using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Godot;
using TheLastGate.Config;
using TheLastGate.Map;

namespace TheLastGate.Assets;

public sealed class SpriteLoader : IDisposable
{
    private readonly Dictionary<int, uint> averageColours = [];
    private readonly BmpLibrary bmpLibrary;
    private readonly SpriteCache<Texture2D> cache;
    private readonly IReadOnlyDictionary<int, string> dataOverrides;
    private readonly MyV2Art? myV2Art;
    private readonly IReadOnlyDictionary<int, string> playerOverrides;
    private readonly PngLibrary pngLibrary;
    private readonly string uiFolder;
    private readonly Dictionary<int, Texture2D?> uiPictures = [];

    public SpriteLoader(string dataPath, long cacheBudgetBytes)
    {
        dataOverrides = SpriteOverrides.Find(Path.Combine(dataPath, DataLayout.Overrides));
        playerOverrides = SpriteOverrides.Find(DataPaths.PlayerOverrides());
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

        try
        {
            myV2Art = MyV2Art.Open(DataPaths.MyV2Client());
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
            GD.PushError($"The MyV2 client's art couldn't be opened: {exception.Message}");
        }

        cache = new SpriteCache<Texture2D>(cacheBudgetBytes, texture => texture.Dispose());
    }

    public IReadOnlyDictionary<int, int> Swaps { get; set; } = new Dictionary<int, int>();

    public void Dispose()
    {
        cache.Clear();
        foreach (var picture in uiPictures.Values)
        {
            picture?.Dispose();
        }

        uiPictures.Clear();
        myV2Art?.Dispose();
        pngLibrary.Dispose();
        bmpLibrary.Dispose();
    }

    public string? MyV2ArtFolder => myV2Art?.Folder;

    public SpriteSource FindSource(int spriteNumber) => Locate(spriteNumber).Source;

    public Texture2D? GetTexture(int shownNumber)
    {
        var spriteNumber = Swaps.GetValueOrDefault(shownNumber, shownNumber);
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

    // Kept apart from the texture cache, so evicting a texture never costs a colour the explored map asks for often.
    public uint AverageColour(int shownNumber)
    {
        var spriteNumber = Swaps.GetValueOrDefault(shownNumber, shownNumber);
        if (!averageColours.TryGetValue(spriteNumber, out var colour))
        {
            colour = LoadImage(spriteNumber) is { } image ? SpriteColours.Average(image.GetData()) : 0;
            averageColours[spriteNumber] = colour;
        }

        return colour;
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

    // A player's own sprite comes first, then the MyV2 client's art, then this client's archives. Interface art asked
    // for at ClientArtBase and up comes only from this client's archives.
    private (SpriteSource Source, int Number) Locate(int spriteNumber)
    {
        if (spriteNumber >= SpriteNumbers.ClientArtBase)
        {
            var clientNumber = spriteNumber - SpriteNumbers.ClientArtBase;
            return (FindClientSource(clientNumber), clientNumber);
        }

        if (playerOverrides.ContainsKey(spriteNumber))
        {
            return (SpriteSource.PlayerOverride, spriteNumber);
        }

        var myV2Source = myV2Art?.FindSource(spriteNumber) ?? SpriteSource.None;
        return (myV2Source != SpriteSource.None ? myV2Source : FindClientSource(spriteNumber), spriteNumber);
    }

    private SpriteSource FindClientSource(int spriteNumber)
    {
        if (dataOverrides.ContainsKey(spriteNumber))
        {
            return SpriteSource.Override;
        }

        if (pngLibrary.Contains(spriteNumber))
        {
            return SpriteSource.PngLibrary;
        }

        return bmpLibrary.Contains(spriteNumber) ? SpriteSource.BmpLibrary : SpriteSource.None;
    }

    private Image? LoadImage(int spriteNumber)
    {
        var (source, number) = Locate(spriteNumber);
        try
        {
            return source switch
            {
                SpriteSource.None => null,
                SpriteSource.PlayerOverride => DecodeOverride(number, playerOverrides[number]),
                SpriteSource.MyV2Loose => DecodeOverride(number, myV2Art!.LooseFile(number)),
                SpriteSource.MyV2PngLibrary => DecodePng(number, myV2Art!.PngLibrary.ReadPng(number)!),
                SpriteSource.MyV2Base => ToImage(myV2Art!.BaseLibrary.Read(number)!),
                SpriteSource.Override => DecodeOverride(number, dataOverrides[number]),
                SpriteSource.PngLibrary => DecodePng(number, pngLibrary.ReadPng(number)!),
                SpriteSource.BmpLibrary => ToImage(bmpLibrary.Read(number)!),
                _ => throw new UnreachableException(),
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
