using GdUnit4;
using TheLastGate.Notes;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Notes;

[TestSuite]
public class NoteCodecTests
{
    [TestCase(0u, new byte[] { 0x00 })]
    [TestCase(127u, new byte[] { 0x7F })]
    [TestCase(128u, new byte[] { 0x80, 0x01 })]
    [TestCase(300u, new byte[] { 0xAC, 0x02 })]
    public void Varints_AreSevenBitsAByteLowFirst(uint value, byte[] encoded)
    {
        var bytes = new List<byte>();
        NoteCodec.WriteVarint(bytes, value);
        AssertThat(bytes).ContainsExactly(encoded);

        var offset = 0;
        AssertThat(NoteCodec.TryReadVarint(encoded, ref offset, out var read)).IsTrue();
        AssertThat(read).IsEqual(value);
        AssertThat(offset).IsEqual(encoded.Length);
    }

    [TestCase]
    public void AVarintThatNeverEnds_CanNotBeRead()
    {
        var offset = 0;

        AssertThat(NoteCodec.TryReadVarint([0x80, 0x80], ref offset, out _)).IsFalse();
    }

    [TestCase]
    public void ACategory_IsWrittenAsTheCurrentClientWritesIt()
    {
        var set = new NoteSet();
        var category = set.AddCategory("Ab", 2);
        set.AddPoint(category, NoteKind.Pin, 3, 1, "P");
        set.AddPoint(category, NoteKind.Pin, 2, 1, "");
        var bytes = new List<byte>();

        NoteCodec.EncodeCategory(set.FindCategory(category)!, bytes);

        AssertThat(bytes)
            .ContainsExactly(new byte[] { 2, (byte)'A', (byte)'b', 2, 2, 1, 1, (byte)'P', 6, 2, 1, 0, 1, 0 });
    }

    [TestCase]
    public void ATree_RoundTripsWithItsNestingFlagsAndColours()
    {
        var set = new NoteSet();
        var aston = set.AddCategory("Aston", 1);
        var givers = set.AddCategory("Quest Givers", 3, aston);
        var route = set.AddCategory("Route", 5);
        var pin = set.AddPoint(givers, NoteKind.Pin, 55, 66, "Elder");
        set.SetNoteColour(pin, 4);
        set.AddPath(route, [new PathSegment(10, 20, 30, 40), new PathSegment(30, 40, 1023, 0)], "Maze");
        set.FindCategory(aston)!.ShowLabels = false;
        set.FindCategory(route)!.Visible = false;
        var bytes = new List<byte>();
        AssertThat(NoteCodec.EncodeTree(set, 0, bytes)).IsTrue();

        var offset = 0;
        AssertThat(NoteCodec.DecodeTree(bytes.ToArray(), ref offset, false, out var tree))
            .IsEqual(NoteDecodeResult.Decoded);

        AssertThat(offset).IsEqual(bytes.Count);
        AssertThat(tree!.Tree().Select(category => category.Name)).ContainsExactly("Aston", "Quest Givers", "Route");
        var loadedAston = tree.Tree()[0];
        var loadedGivers = tree.Tree()[1];
        var loadedRoute = tree.Tree()[2];
        AssertThat(loadedGivers.ParentId).IsEqual(loadedAston.Id);
        AssertThat(loadedAston.ShowLabels).IsFalse();
        AssertThat(loadedRoute.Visible).IsFalse();
        AssertThat(loadedGivers.ColourIndex).IsEqual(3);
        AssertThat(loadedGivers.Notes[0].ColourIndex).IsEqual(4);
        AssertThat((loadedGivers.Notes[0].TileX, loadedGivers.Notes[0].TileY)).IsEqual((55, 66));
        AssertThat(loadedRoute.Notes[0].ColourIndex).IsEqual(Note.CategoryColour);
        AssertThat(loadedRoute.Notes[0].Segments)
            .ContainsExactly(new PathSegment(10, 20, 30, 40), new PathSegment(30, 40, 1023, 0));
        AssertThat(tree.Changed).IsFalse();
    }

    [TestCase]
    public void EncodeTree_FromOneRoot_TakesOnlyItsSubtree()
    {
        var set = new NoteSet();
        var aston = set.AddCategory("Aston", 1);
        set.AddCategory("Quest Givers", 3, aston);
        set.AddCategory("Neiseer", 2);
        var bytes = new List<byte>();
        AssertThat(NoteCodec.EncodeTree(set, aston, bytes)).IsTrue();

        var offset = 0;
        NoteCodec.DecodeTree(bytes.ToArray(), ref offset, true, out var tree);

        AssertThat(tree!.Tree().Select(category => category.Name)).ContainsExactly("Aston", "Quest Givers");
        AssertThat(NoteCodec.EncodeTree(set, 999, [])).IsFalse();
    }

    [TestCase]
    public void DecodeTree_WithOneRootWanted_RefusesTwoOrNone()
    {
        var set = new NoteSet();
        set.AddCategory("A", 0);
        set.AddCategory("B", 0);
        var twoRoots = new List<byte>();
        NoteCodec.EncodeTree(set, 0, twoRoots);

        var offset = 0;
        AssertThat(NoteCodec.DecodeTree(twoRoots.ToArray(), ref offset, true, out _)).IsEqual(NoteDecodeResult.Damaged);
        offset = 0;
        AssertThat(NoteCodec.DecodeTree([0], ref offset, true, out _)).IsEqual(NoteDecodeResult.Damaged);
    }

    [TestCase]
    public void DecodeTree_RefusesAParentThatComesLater()
    {
        byte[] bytes = [1, 1, 3, 1, (byte)'A', 0, 0];
        var offset = 0;

        AssertThat(NoteCodec.DecodeTree(bytes, ref offset, false, out _)).IsEqual(NoteDecodeResult.Damaged);
    }

    [TestCase]
    public void DecodeTree_CallsTooManyCategoriesTooLarge()
    {
        var bytes = new List<byte>();
        NoteCodec.WriteVarint(bytes, NoteLimits.Categories + 1);
        var offset = 0;

        AssertThat(NoteCodec.DecodeTree(bytes.ToArray(), ref offset, false, out _)).IsEqual(NoteDecodeResult.TooLarge);
    }

    [TestCase]
    public void DecodeCategory_NamesANamelessCategoryImportedAndRepairsItsColour()
    {
        byte[] bytes = [0, 99, 0];
        var offset = 0;

        AssertThat(NoteCodec.DecodeCategory(bytes, ref offset, out var category)).IsEqual(NoteDecodeResult.Decoded);

        AssertThat(category!.Name).IsEqual("Imported");
        AssertThat(category.ColourIndex).IsEqual(0);
        AssertThat(category.Visible).IsTrue();
        AssertThat(category.ShowLabels).IsTrue();
    }

    [TestCase(new byte[] { 1, (byte)'A', 0, 1, 4, 0, 0, 0 })]
    [TestCase(new byte[] { 1, (byte)'A', 0, 1, 2, 0, 0, 0 })]
    [TestCase(new byte[] { 1, (byte)'A', 0, 1, 3, 0, 0 })]
    [TestCase(new byte[] { 1, (byte)'A', 0, 1, 1, 0, 0x80, 0x40, 0 })]
    [TestCase(new byte[] { 1, (byte)'A', 0, 1, 1, 0 })]
    public void DecodeCategory_CallsBrokenNotesDamaged(byte[] bytes)
    {
        var offset = 0;

        AssertThat(NoteCodec.DecodeCategory(bytes, ref offset, out _)).IsEqual(NoteDecodeResult.Damaged);
    }

    [TestCase]
    public void DecodeCategory_CallsTooManyNotesTooLarge()
    {
        var bytes = new List<byte> { 1, (byte)'A', 0 };
        NoteCodec.WriteVarint(bytes, NoteLimits.NotesPerCategory + 1);
        var offset = 0;

        AssertThat(NoteCodec.DecodeCategory(bytes.ToArray(), ref offset, out _)).IsEqual(NoteDecodeResult.TooLarge);
    }

    [TestCase]
    public void DecodeCategory_RefusesALabelLongerThanTheLimit()
    {
        var bytes = new List<byte> { 1, (byte)'A', 0, 1, 1, NoteLimits.LabelBytes + 1 };
        bytes.AddRange(Enumerable.Repeat((byte)'x', NoteLimits.LabelBytes + 1));
        bytes.AddRange([0, 0]);
        var offset = 0;

        AssertThat(NoteCodec.DecodeCategory(bytes.ToArray(), ref offset, out _)).IsEqual(NoteDecodeResult.Damaged);
    }
}
