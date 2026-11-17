using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class NoteCodecTests
{
    [Theory]
    [InlineData(0u, new byte[] { 0x00 })]
    [InlineData(127u, new byte[] { 0x7F })]
    [InlineData(128u, new byte[] { 0x80, 0x01 })]
    [InlineData(300u, new byte[] { 0xAC, 0x02 })]
    public void Varints_AreSevenBitsAByteLowFirst(uint value, byte[] encoded)
    {
        var bytes = new List<byte>();
        NoteCodec.WriteVarint(bytes, value);
        Assert.Equal(encoded, bytes);

        var offset = 0;
        Assert.True(NoteCodec.TryReadVarint(encoded, ref offset, out var read));
        Assert.Equal(value, read);
        Assert.Equal(encoded.Length, offset);
    }

    [Fact]
    public void AVarintThatNeverEnds_CanNotBeRead()
    {
        var offset = 0;

        Assert.False(NoteCodec.TryReadVarint([0x80, 0x80], ref offset, out _));
    }

    [Fact]
    public void ACategory_IsWrittenAsTheCurrentClientWritesIt()
    {
        var set = new NoteSet();
        var category = set.AddCategory("Ab", 2);
        set.AddPoint(category, NoteKind.Pin, 3, 1, "P");
        set.AddPoint(category, NoteKind.Pin, 2, 1, "");
        var bytes = new List<byte>();

        NoteCodec.EncodeCategory(set.FindCategory(category)!, bytes);

        Assert.Equal(
            new byte[] { 2, (byte)'A', (byte)'b', 2, 2, 1, 1, (byte)'P', 6, 2, 1, 0, 1, 0 },
            bytes);
    }

    [Fact]
    public void ATree_RoundTripsWithItsNestingFlagsAndColours()
    {
        var set = new NoteSet();
        var aston = set.AddCategory("Aston", 1);
        var givers = set.AddCategory("Quest Givers", 3, aston);
        var route = set.AddCategory("Route", 5);
        var pin = set.AddPoint(givers, NoteKind.Pin, 55, 66, "Elder");
        set.SetNoteColour(pin, 4);
        set.AddPath(route, [new PathSegment(10, 20, 30, 40), new PathSegment(30, 40, 2047, 0)], "Maze");
        set.FindCategory(aston)!.ShowLabels = false;
        set.FindCategory(route)!.Visible = false;
        var bytes = new List<byte>();
        Assert.True(NoteCodec.EncodeTree(set, 0, bytes));

        var offset = 0;
        Assert.Equal(NoteDecodeResult.Decoded, NoteCodec.DecodeTree(bytes.ToArray(), ref offset, false, out var tree));

        Assert.Equal(bytes.Count, offset);
        Assert.Equal(["Aston", "Quest Givers", "Route"], tree!.Tree().Select(category => category.Name));
        var loadedAston = tree.Tree()[0];
        var loadedGivers = tree.Tree()[1];
        var loadedRoute = tree.Tree()[2];
        Assert.Equal(loadedAston.Id, loadedGivers.ParentId);
        Assert.False(loadedAston.ShowLabels);
        Assert.False(loadedRoute.Visible);
        Assert.Equal(3, loadedGivers.ColourIndex);
        Assert.Equal(4, loadedGivers.Notes[0].ColourIndex);
        Assert.Equal((55, 66), (loadedGivers.Notes[0].TileX, loadedGivers.Notes[0].TileY));
        Assert.Equal(Note.CategoryColour, loadedRoute.Notes[0].ColourIndex);
        Assert.Equal(
            [new PathSegment(10, 20, 30, 40), new PathSegment(30, 40, 2047, 0)],
            loadedRoute.Notes[0].Segments);
        Assert.False(tree.Changed);
    }

    [Fact]
    public void EncodeTree_FromOneRoot_TakesOnlyItsSubtree()
    {
        var set = new NoteSet();
        var aston = set.AddCategory("Aston", 1);
        set.AddCategory("Quest Givers", 3, aston);
        set.AddCategory("Neiseer", 2);
        var bytes = new List<byte>();
        Assert.True(NoteCodec.EncodeTree(set, aston, bytes));

        var offset = 0;
        NoteCodec.DecodeTree(bytes.ToArray(), ref offset, true, out var tree);

        Assert.Equal(["Aston", "Quest Givers"], tree!.Tree().Select(category => category.Name));
        Assert.False(NoteCodec.EncodeTree(set, 999, []));
    }

    [Fact]
    public void DecodeTree_WithOneRootWanted_RefusesTwoOrNone()
    {
        var set = new NoteSet();
        set.AddCategory("A", 0);
        set.AddCategory("B", 0);
        var twoRoots = new List<byte>();
        NoteCodec.EncodeTree(set, 0, twoRoots);

        var offset = 0;
        Assert.Equal(NoteDecodeResult.Damaged, NoteCodec.DecodeTree(twoRoots.ToArray(), ref offset, true, out _));
        offset = 0;
        Assert.Equal(NoteDecodeResult.Damaged, NoteCodec.DecodeTree([0], ref offset, true, out _));
    }

    [Fact]
    public void DecodeTree_RefusesAParentThatComesLater()
    {
        byte[] bytes = [1, 1, 3, 1, (byte)'A', 0, 0];
        var offset = 0;

        Assert.Equal(NoteDecodeResult.Damaged, NoteCodec.DecodeTree(bytes, ref offset, false, out _));
    }

    [Fact]
    public void DecodeTree_CallsTooManyCategoriesTooLarge()
    {
        var bytes = new List<byte>();
        NoteCodec.WriteVarint(bytes, NoteLimits.Categories + 1);
        var offset = 0;

        Assert.Equal(NoteDecodeResult.TooLarge, NoteCodec.DecodeTree(bytes.ToArray(), ref offset, false, out _));
    }

    [Fact]
    public void DecodeCategory_NamesANamelessCategoryImportedAndRepairsItsColour()
    {
        byte[] bytes = [0, 99, 0];
        var offset = 0;

        Assert.Equal(NoteDecodeResult.Decoded, NoteCodec.DecodeCategory(bytes, ref offset, out var category));

        Assert.Equal("Imported", category!.Name);
        Assert.Equal(0, category.ColourIndex);
        Assert.True(category.Visible);
        Assert.True(category.ShowLabels);
    }

    [Theory]
    [InlineData(new byte[] { 1, (byte)'A', 0, 1, 4, 0, 0, 0 })]
    [InlineData(new byte[] { 1, (byte)'A', 0, 1, 2, 0, 0, 0 })]
    [InlineData(new byte[] { 1, (byte)'A', 0, 1, 3, 0, 0 })]
    [InlineData(new byte[] { 1, (byte)'A', 0, 1, 1, 0, 0x80, 0x40, 0 })]
    [InlineData(new byte[] { 1, (byte)'A', 0, 1, 1, 0 })]
    public void DecodeCategory_CallsBrokenNotesDamaged(byte[] bytes)
    {
        var offset = 0;

        Assert.Equal(NoteDecodeResult.Damaged, NoteCodec.DecodeCategory(bytes, ref offset, out _));
    }

    [Fact]
    public void DecodeCategory_CallsTooManyNotesTooLarge()
    {
        var bytes = new List<byte> { 1, (byte)'A', 0 };
        NoteCodec.WriteVarint(bytes, NoteLimits.NotesPerCategory + 1);
        var offset = 0;

        Assert.Equal(NoteDecodeResult.TooLarge, NoteCodec.DecodeCategory(bytes.ToArray(), ref offset, out _));
    }

    [Fact]
    public void DecodeCategory_RefusesALabelLongerThanTheLimit()
    {
        var bytes = new List<byte> { 1, (byte)'A', 0, 1, 1, NoteLimits.LabelBytes + 1 };
        bytes.AddRange(Enumerable.Repeat((byte)'x', NoteLimits.LabelBytes + 1));
        bytes.AddRange([0, 0]);
        var offset = 0;

        Assert.Equal(NoteDecodeResult.Damaged, NoteCodec.DecodeCategory(bytes.ToArray(), ref offset, out _));
    }
}
