using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using TheLastGate.Notes;

namespace TheLastGate.Tests;

public sealed class NoteStoreTests : IDisposable
{
    private const int HeaderSize = 16;

    private readonly string folder = Directory.CreateTempSubdirectory("note-store-").FullName;

    public void Dispose() => Directory.Delete(folder, true);

    [Fact]
    public void Save_WritesTheCurrentClientsHeaderThenZlib()
    {
        var file = NoteStore.Save(Sample());

        Assert.Equal("TLGA"u8.ToArray(), file[..4]);
        Assert.Equal(3u, BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(4)));
        var payloadSize = BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(8));
        Assert.Equal((uint)(file.Length - HeaderSize), BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(12)));
        var payload = Inflate(file[HeaderSize..]);
        Assert.Equal(payloadSize, (uint)payload.Length);
        Assert.Equal(1, payload[0]);
    }

    [Fact]
    public void ASavedSet_LoadsBackAsItWas()
    {
        var loaded = NoteStore.Load(NoteStore.Save(Sample()))!;

        Assert.False(loaded.Changed);
        Assert.True(loaded.ShowOnMinimap);
        Assert.Equal(["Keys", "Route"], loaded.Tree().Select(category => category.Name));
        var keys = loaded.Tree()[0];
        var route = loaded.Tree()[1];
        Assert.True(keys.Visible);
        Assert.False(keys.ShowLabels);
        Assert.False(route.Visible);
        Assert.True(route.ShowLabels);
        Assert.Equal(2, keys.ColourIndex);
        Assert.Equal(5, route.ColourIndex);
        Assert.Equal(
            [(NoteKind.Pin, "Red key", 100, 200), (NoteKind.Text, "Beware", 300, 400)],
            keys.Notes.Select(note => (note.Kind, note.Label, note.TileX, note.TileY)));
        Assert.Equal("Maze", route.Notes[0].Label);
        Assert.Equal(3, route.Notes[0].Segments.Count);
        Assert.Equal(90, route.Notes[0].Segments[2].EndY);
    }

    [Fact]
    public void Load_KeepsTheNestingAndCousinsWithTheSameName()
    {
        var saved = new NoteSet { ShowOnMinimap = true };
        var aston = saved.AddCategory("Aston", 1);
        var neiseer = saved.AddCategory("Neiseer", 2);
        saved.AddCategory("Quest Givers", 3, aston);
        var neiseerGivers = saved.AddCategory("Quest Givers", 6, neiseer);
        saved.AddPoint(neiseerGivers, NoteKind.Pin, 55, 66, "Elder");

        var loaded = NoteStore.Load(NoteStore.Save(saved))!;

        Assert.Equal(
            ["Aston", "Quest Givers", "Neiseer", "Quest Givers"],
            loaded.Tree().Select(category => category.Name));
        var elder = loaded.Tree()[3];
        Assert.Equal(6, elder.ColourIndex);
        Assert.Equal("Neiseer", loaded.FindCategory(elder.ParentId)!.Name);
        Assert.Equal((55, 66), (elder.Notes[0].TileX, elder.Notes[0].TileY));
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("TLGA")]
    public void Load_OfAnUnreadableFile_GivesNothing(string contents)
    {
        Assert.Null(NoteStore.Load(Encoding.ASCII.GetBytes(contents)));
    }

    [Fact]
    public void Load_RefusesBytesLeftOverAfterTheTree()
    {
        var payload = new List<byte> { 0 };
        NoteCodec.EncodeTree(Sample(), 0, payload);
        payload.Add(0);

        Assert.Null(NoteStore.Load(StoreFile(3, payload.ToArray())));
    }

    [Fact]
    public void AFormatOneFile_LoadsWithLabelsShown()
    {
        var source = new NoteSet();
        var old = source.AddCategory("Old", 3);
        source.AddPoint(old, NoteKind.Pin, 12, 34, "Gate");

        var loaded = NoteStore.Load(LegacyFile(1, source))!;

        var category = Assert.Single(loaded.Categories);
        Assert.Equal("Old", category.Name);
        Assert.True(category.ShowLabels);
        Assert.Single(category.Notes);
        Assert.True(loaded.ShowOnMinimap);
    }

    [Fact]
    public void AFormatTwoFile_KeepsItsFlagsAndNamesThatClash()
    {
        var source = new NoteSet();
        source.FindCategory(source.AddCategory("Aston", 3))!.ShowLabels = false;
        var lower = source.Attach(NoteCategory.Detached("aston", 4, false, true, []), 0);

        var loaded = NoteStore.Load(LegacyFile(2, source))!;

        Assert.Equal(["Aston", "aston"], loaded.Categories.Select(category => category.Name));
        Assert.Equal([3, 4], loaded.Categories.Select(category => category.ColourIndex));
        Assert.Equal([true, false], loaded.Categories.Select(category => category.Visible));
        Assert.Equal([false, true], loaded.Categories.Select(category => category.ShowLabels));
        Assert.All(loaded.Categories, category => Assert.Equal(0, category.ParentId));
        Assert.NotEqual(0, lower);
    }

    [Fact]
    public void SaveTo_WritesTheFileAndMarksTheSetSaved()
    {
        var path = Path.Combine(folder, "TLGAnnotations_test_1.dat");
        var set = Sample();

        Assert.True(NoteStore.SaveTo(path, set));

        Assert.False(set.Changed);
        Assert.False(File.Exists(path + ".tmp"));
        Assert.Equal(["Keys", "Route"], NoteStore.Load(File.ReadAllBytes(path))!.Tree().Select(c => c.Name));
    }

    [Fact]
    public void Open_LoadsTheOwnFile()
    {
        var own = Path.Combine(folder, "own.dat");
        NoteStore.SaveTo(own, Sample());

        var (set, setAside) = NoteStore.Open(own, Path.Combine(folder, "sdl.dat"));

        Assert.Equal(["Keys", "Route"], set.Tree().Select(category => category.Name));
        Assert.False(setAside);
        Assert.False(set.Changed);
    }

    [Fact]
    public void Open_ImportsTheCurrentClientsFileWhenThereIsNoOwnFile()
    {
        var sdlClient = Path.Combine(folder, "sdl.dat");
        NoteStore.SaveTo(sdlClient, Sample());

        var (set, _) = NoteStore.Open(Path.Combine(folder, "own.dat"), sdlClient);

        Assert.Equal(["Keys", "Route"], set.Tree().Select(category => category.Name));
    }

    [Fact]
    public void Open_StartsWithMyNotesWhenThereIsNothingToLoad()
    {
        var (set, setAside) = NoteStore.Open(Path.Combine(folder, "own.dat"), Path.Combine(folder, "sdl.dat"));

        var category = Assert.Single(set.Categories);
        Assert.Equal("My notes", category.Name);
        Assert.Equal(1, category.ColourIndex);
        Assert.False(set.Changed);
        Assert.False(setAside);
    }

    [Fact]
    public void Open_SetsADamagedOwnFileAsideReplacingAnOlderOne()
    {
        var own = Path.Combine(folder, "own.dat");
        File.WriteAllText(own + ".bad", "older");
        File.WriteAllText(own, "garbage");

        var (set, setAside) = NoteStore.Open(own, Path.Combine(folder, "sdl.dat"));

        Assert.True(setAside);
        Assert.False(File.Exists(own));
        Assert.Equal("garbage", File.ReadAllText(own + ".bad"));
        Assert.Equal("My notes", Assert.Single(set.Categories).Name);
    }

    [Fact]
    public void Open_NeverMovesTheCurrentClientsFile()
    {
        var sdlClient = Path.Combine(folder, "sdl.dat");
        File.WriteAllText(sdlClient, "garbage");

        var (set, setAside) = NoteStore.Open(Path.Combine(folder, "own.dat"), sdlClient);

        Assert.False(setAside);
        Assert.Equal("garbage", File.ReadAllText(sdlClient));
        Assert.False(File.Exists(sdlClient + ".bad"));
        Assert.Equal("My notes", Assert.Single(set.Categories).Name);
    }

    private static NoteSet Sample()
    {
        var set = new NoteSet { ShowOnMinimap = true };
        var keys = set.AddCategory("Keys", 2);
        var route = set.AddCategory("Route", 5);
        set.AddPoint(keys, NoteKind.Pin, 100, 200, "Red key");
        set.AddPoint(keys, NoteKind.Text, 300, 400, "Beware");
        set.AddPath(
            route,
            [new PathSegment(10, 20, 30, 40), new PathSegment(30, 40, 50, 60), new PathSegment(50, 60, 70, 90)],
            "Maze");
        set.FindCategory(keys)!.ShowLabels = false;
        set.FindCategory(route)!.Visible = false;
        return set;
    }

    private static byte[] LegacyFile(uint format, NoteSet source)
    {
        var payload = new List<byte> { 1 };
        NoteCodec.WriteVarint(payload, (uint)source.Categories.Count);
        foreach (var category in source.Categories)
        {
            payload.Add(category.Visible ? (byte)1 : (byte)0);
            if (format >= 2)
            {
                payload.Add(category.ShowLabels ? (byte)1 : (byte)0);
            }

            NoteCodec.EncodeCategory(category, payload);
        }

        return StoreFile(format, payload.ToArray());
    }

    private static byte[] StoreFile(uint format, byte[] payload)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true))
        {
            zlib.Write(payload);
        }

        var file = new byte[HeaderSize + compressed.Length];
        "TLGA"u8.CopyTo(file);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(4), format);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(8), (uint)payload.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(12), (uint)compressed.Length);
        compressed.ToArray().CopyTo(file, HeaderSize);
        return file;
    }

    private static byte[] Inflate(byte[] compressed)
    {
        using var zlib = new ZLibStream(new MemoryStream(compressed), CompressionMode.Decompress);
        using var payload = new MemoryStream();
        zlib.CopyTo(payload);
        return payload.ToArray();
    }
}
