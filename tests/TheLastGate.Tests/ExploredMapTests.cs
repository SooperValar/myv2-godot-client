using System.Buffers.Binary;
using System.IO.Compression;
using TheLastGate.Map;

namespace TheLastGate.Tests;

public sealed class ExploredMapTests : IDisposable
{
    private const int Width = 64;
    private const int Height = 32;
    private const int HeaderSize = 20;

    private readonly string folder = Directory.CreateTempSubdirectory("explored-map-").FullName;

    public void Dispose() => Directory.Delete(folder, true);

    [Fact]
    public void ANewMap_IsTheCurrentClientsSizeAndUnexplored()
    {
        var map = new ExploredMap();

        Assert.Equal(2048, map.Width);
        Assert.Equal(2048, map.Height);
        Assert.Equal(0u, map.ColourAt(1000, 1000));
        Assert.False(map.Changed);
    }

    [Fact]
    public void SetTile_MarksTheMapChangedOnlyWhenTheColourDiffers()
    {
        var map = new ExploredMap(Width, Height);

        map.SetTile(3, 4, 0x1234);
        Assert.Equal(0x1234u, map.ColourAt(3, 4));
        Assert.True(map.Changed);

        map.MarkSaved();
        map.SetTile(3, 4, 0x1234);
        Assert.False(map.Changed);

        map.SetTile(3, 4, 0x4321);
        Assert.Equal(0x4321u, map.ColourAt(3, 4));
        Assert.True(map.Changed);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(Width, 0)]
    [InlineData(0, Height)]
    public void SetTile_IgnoresTilesOffTheMap(int tileX, int tileY)
    {
        var map = new ExploredMap(Width, Height);

        map.SetTile(tileX, tileY, 0x1111);

        Assert.False(map.Changed);
        Assert.Equal(0, map.Revision);
    }

    [Fact]
    public void Revision_CountsEveryChangeAndEveryLoad()
    {
        var map = new ExploredMap(Width, Height);

        map.SetTile(1, 1, 0x0101);
        Assert.Equal(1, map.Revision);
        map.SetTile(1, 1, 0x0101);
        Assert.Equal(1, map.Revision);

        map.Load([]);
        Assert.Equal(2, map.Revision);
    }

    [Fact]
    public void ClearBox_WipesOnlyTheBox()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(1, 1, 0x0101);
        map.SetTile(2, 2, 0x0202);
        map.SetTile(5, 5, 0x0505);
        map.MarkSaved();

        map.ClearBox(1, 1, 2, 2);

        Assert.Equal(0u, map.ColourAt(1, 1));
        Assert.Equal(0u, map.ColourAt(2, 2));
        Assert.Equal(0x0505u, map.ColourAt(5, 5));
        Assert.True(map.Changed);
    }

    [Fact]
    public void ClearBox_StaysInsideTheMap()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(Width - 1, Height - 1, 0x7777);

        map.ClearBox(Width - 1, Height - 1, 1000, 1000);
        map.ClearBox(-10, -10, 5, 5);

        Assert.Equal(0u, map.ColourAt(Width - 1, Height - 1));
    }

    [Fact]
    public void ClearBox_OfUnexploredTiles_IsNotAChange()
    {
        var map = new ExploredMap(Width, Height);

        map.ClearBox(0, 0, 4, 4);

        Assert.False(map.Changed);
    }

    [Fact]
    public void Save_WritesTheCurrentClientsHeaderThenZlibTiles()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(Width - 1, Height - 1, 0xABCDEF);

        var file = map.Save();

        Assert.Equal("TLGM"u8.ToArray(), file[..4]);
        Assert.Equal(2u, BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(4)));
        Assert.Equal((uint)Width, BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(8)));
        Assert.Equal((uint)Height, BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(12)));
        Assert.Equal((uint)(file.Length - HeaderSize), BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(16)));
        var tiles = Inflate(file[HeaderSize..]);
        Assert.Equal(Width * Height * sizeof(uint), tiles.Length);
        Assert.Equal(0xABCDEFu, BinaryPrimitives.ReadUInt32LittleEndian(tiles.AsSpan(tiles.Length - sizeof(uint))));
    }

    [Fact]
    public void SavedTiles_AreColumnMajor()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(1, 0, 0x010000);

        var tiles = Inflate(map.Save()[HeaderSize..]);

        Assert.Equal(0x010000u, BinaryPrimitives.ReadUInt32LittleEndian(tiles.AsSpan(Height * sizeof(uint))));
    }

    [Fact]
    public void ASavedMap_LoadsBackUnchanged()
    {
        var saved = new ExploredMap(Width, Height);
        for (var tileX = 0; tileX < Width; tileX++)
        {
            saved.SetTile(tileX, tileX % Height, (uint)(tileX * 0x010203));
        }

        var loaded = new ExploredMap(Width, Height);
        Assert.True(loaded.Load(saved.Save()));

        for (var tileX = 0; tileX < Width; tileX++)
        {
            Assert.Equal(saved.ColourAt(tileX, tileX % Height), loaded.ColourAt(tileX, tileX % Height));
        }

        Assert.False(loaded.Changed);
    }

    [Fact]
    public void Load_OfAnEmptyOrGarbageFile_GivesABlankMap()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(1, 1, 0x0101);

        Assert.False(map.Load([]));
        Assert.Equal(0u, map.ColourAt(1, 1));

        map.SetTile(1, 1, 0x0101);
        Assert.False(map.Load("not a map file at all"u8));
        Assert.Equal(0u, map.ColourAt(1, 1));
        Assert.False(map.Changed);
    }

    [Fact]
    public void Load_OfATruncatedFile_GivesABlankMap()
    {
        var saved = new ExploredMap(Width, Height);
        saved.SetTile(2, 2, 0x0202);
        var file = saved.Save();

        var loaded = new ExploredMap(Width, Height);

        Assert.False(loaded.Load(file[..^4]));
        Assert.Equal(0u, loaded.ColourAt(2, 2));
    }

    [Fact]
    public void Load_OfAMapOfAnotherSize_GivesABlankMap()
    {
        var saved = new ExploredMap(Width, Height);
        saved.SetTile(2, 2, 0x0202);

        var loaded = new ExploredMap(Height, Width);

        Assert.False(loaded.Load(saved.Save()));
        Assert.Equal(0u, loaded.ColourAt(2, 2));
    }

    [Fact]
    public void Load_OfAnUnknownFormat_GivesABlankMap()
    {
        var saved = new ExploredMap(Width, Height);
        saved.SetTile(2, 2, 0x0202);
        var file = saved.Save();
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(4), 99);

        var loaded = new ExploredMap(Width, Height);

        Assert.False(loaded.Load(file));
        Assert.Equal(0u, loaded.ColourAt(2, 2));
    }

    [Fact]
    public void Load_ExpandsAnOldRgb555FileToFullColour()
    {
        var packed = new byte[Width * Height * sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(packed.Length - sizeof(ushort)), 31 << 10);
        BinaryPrimitives.WriteUInt16LittleEndian(packed, 0b10000_00000_00001);

        var map = new ExploredMap(Width, Height);

        Assert.True(map.Load(MapFile(1, Width, Height, packed)));
        Assert.Equal(0xFF0000u, map.ColourAt(Width - 1, Height - 1));
        Assert.Equal(0x840008u, map.ColourAt(0, 0));
    }

    [Fact]
    public void SaveTo_WritesTheFileAndMarksTheMapSaved()
    {
        var path = Path.Combine(folder, "TLGMap_test_1.dat");
        var map = new ExploredMap(Width, Height);
        map.SetTile(2, 2, 0x0202);

        Assert.True(map.SaveTo(path));

        Assert.False(map.Changed);
        Assert.False(File.Exists(path + ".tmp"));
        var loaded = new ExploredMap(Width, Height);
        Assert.True(loaded.LoadFrom(path));
        Assert.Equal(0x0202u, loaded.ColourAt(2, 2));
    }

    [Fact]
    public void SaveTo_ReplacesAnEarlierSave()
    {
        var path = Path.Combine(folder, "TLGMap_test_1.dat");
        var map = new ExploredMap(Width, Height);
        map.SetTile(2, 2, 0x0202);
        map.SaveTo(path);

        map.SetTile(2, 2, 0x0303);
        Assert.True(map.SaveTo(path));

        var loaded = new ExploredMap(Width, Height);
        loaded.LoadFrom(path);
        Assert.Equal(0x0303u, loaded.ColourAt(2, 2));
    }

    [Fact]
    public void LoadFrom_AMissingFile_GivesABlankMap()
    {
        var map = new ExploredMap(Width, Height);
        map.SetTile(1, 1, 0x0101);

        Assert.False(map.LoadFrom(Path.Combine(folder, "missing.dat")));

        Assert.Equal(0u, map.ColourAt(1, 1));
    }

    [Fact]
    public void Delete_RemovesTheFileAndAnyHalfWrittenOne()
    {
        var path = Path.Combine(folder, "TLGMap_test_1.dat");
        new ExploredMap(Width, Height).SaveTo(path);
        File.WriteAllText(path + ".tmp", "half written");

        Assert.True(ExploredMap.Delete(path));

        Assert.False(File.Exists(path));
        Assert.False(File.Exists(path + ".tmp"));
        Assert.True(ExploredMap.Delete(path));
    }

    private static byte[] MapFile(uint format, int width, int height, byte[] tiles)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, true))
        {
            zlib.Write(tiles);
        }

        var file = new byte[HeaderSize + compressed.Length];
        "TLGM"u8.CopyTo(file);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(4), format);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(8), (uint)width);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(12), (uint)height);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(16), (uint)compressed.Length);
        compressed.ToArray().CopyTo(file, HeaderSize);
        return file;
    }

    private static byte[] Inflate(byte[] compressed)
    {
        using var zlib = new ZLibStream(new MemoryStream(compressed), CompressionMode.Decompress);
        using var tiles = new MemoryStream();
        zlib.CopyTo(tiles);
        return tiles.ToArray();
    }
}
