using System.IO.Compression;
using System.Text;
using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class NoteShareCodeTests
{
    [Fact]
    public void Export_WritesATreeCodeThatImportsBack()
    {
        var (set, dungeons, _) = DungeonSet();

        var code = NoteShareCode.Export(set, dungeons)!;

        Assert.StartsWith("TLGMAP2:", code);
        Assert.Equal(NoteCodeStatus.Ok, NoteShareCode.Import(code, out var imported));
        Assert.False(imported!.Changed);
        Assert.Equal(["Dungeons", "Cold Cavern"], imported.Tree().Select(category => category.Name));
        var root = imported.Tree()[0];
        var cavern = imported.Tree()[1];
        Assert.Equal(0, root.ParentId);
        Assert.False(root.ShowLabels);
        Assert.Equal(root.Id, cavern.ParentId);
        Assert.False(cavern.Visible);
        Assert.Equal(2, cavern.Notes.Count);
        Assert.Equal(2, cavern.Notes[0].ColourIndex);
    }

    [Fact]
    public void Export_OfNoCategoryOrAnUnknownOne_GivesNothing()
    {
        var (set, _, _) = DungeonSet();

        Assert.Null(NoteShareCode.Export(set, 0));
        Assert.Null(NoteShareCode.Export(set, 9999));
    }

    [Fact]
    public void ACodeBrokenUpWithSpacesAndLines_StillImports()
    {
        var (set, dungeons, _) = DungeonSet();
        var code = NoteShareCode.Export(set, dungeons)!;
        var spaced = " " + string.Join(" \n", code.Chunk(40).Select(part => new string(part))) + " ";

        Assert.Equal(NoteCodeStatus.Ok, NoteShareCode.Import(spaced, out var imported));
        Assert.Equal(2, imported!.Categories.Count);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("TLGMAP3:eNoDAAAAAAE=")]
    [InlineData("")]
    public void TextWithoutAKnownPrefix_IsNotACode(string text)
    {
        Assert.Equal(NoteCodeStatus.NotACode, NoteShareCode.Import(text, out var imported));
        Assert.Null(imported);
    }

    [Fact]
    public void ACodeCutShort_IsDamaged()
    {
        var (set, dungeons, _) = DungeonSet();
        var code = NoteShareCode.Export(set, dungeons)!;
        var shorter = code[..^8];

        Assert.Equal(0, (shorter.Length - "TLGMAP2:".Length) % 4);
        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(shorter, out _));
        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(shorter[..^2], out _));
    }

    [Fact]
    public void ACoordinateOffTheMap_IsDamaged()
    {
        var category = Header("Bad", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 3000);
        Zigzag(category, 0);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(LegacyCode(category), out _));
    }

    [Fact]
    public void AHugeStep_IsDamaged()
    {
        var category = Header("Huge delta", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        NoteCodec.WriteVarint(category, 0xFFFFFFFE);
        Zigzag(category, 0);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(LegacyCode(category), out _));
    }

    [Fact]
    public void AHugeNoteCount_IsTooLarge()
    {
        Assert.Equal(NoteCodeStatus.TooLarge, NoteShareCode.Import(LegacyCode(Header("Huge", 100000)), out _));
    }

    [Fact]
    public void ATypicalMazeGuide_FitsInOneDiscordMessage()
    {
        var set = new NoteSet();
        var guide = set.AddCategory("Maze guide", 1);
        var segments = new List<PathSegment>();
        var (tileX, tileY) = (400, 400);
        for (var index = 0; index < 150; index++)
        {
            var next = index % 2 == 0 ? (tileX + 3, tileY) : (tileX, tileY + 2);
            segments.Add(new PathSegment(tileX, tileY, next.Item1, next.Item2));
            (tileX, tileY) = next;
        }

        set.AddPath(guide, segments, "Maze");
        for (var key = 1; key <= 12; key++)
        {
            set.AddPoint(guide, NoteKind.Pin, 400 + key * 7, 400 + key * 5, $"Key {key}");
        }

        Assert.True(NoteShareCode.Export(set, guide)!.Length < NoteShareCode.DiscordLimit);
    }

    [Fact]
    public void ACategoryOverItsSegmentLimit_IsTooLarge()
    {
        var pathsAtLimit = NoteLimits.SegmentsPerCategory / NoteLimits.SegmentsPerPath;

        Assert.Equal(NoteCodeStatus.Ok, NoteShareCode.Import(FloodCode(pathsAtLimit), out _));
        Assert.Equal(NoteCodeStatus.TooLarge, NoteShareCode.Import(FloodCode(pathsAtLimit + 1), out _));
    }

    [Fact]
    public void ANamelessCategory_ImportsAsImported()
    {
        var category = Header("", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);

        Assert.Equal(NoteCodeStatus.Ok, NoteShareCode.Import(LegacyCode(category), out var imported));
        Assert.Equal("Imported", imported!.Categories[0].Name);
        Assert.Equal("", imported.Categories[0].Notes[0].Label);
    }

    [Fact]
    public void TextWithoutALabel_IsDamaged()
    {
        var category = Header("Notes", 1);
        category.AddRange([(byte)NoteKind.Text, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(LegacyCode(category), out _));
    }

    [Fact]
    public void BytesAfterTheCategory_AreDamaged()
    {
        var category = Header("Keys", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);
        category.Add(0);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(LegacyCode(category), out _));
    }

    [Fact]
    public void AFirstVersionCodeFromTheCurrentClient_StillImports()
    {
        const string code = "TLGMAP1:eNpj8U6tLGZiYmQPSk1RyE6tPME4gZmZxTexKpVZg4FBDowYGOQAuoUHpg==";

        Assert.Equal(NoteCodeStatus.Ok, NoteShareCode.Import(code, out var imported));

        var keys = Assert.Single(imported!.Categories);
        Assert.Equal("Keys", keys.Name);
        Assert.Equal(2, keys.ColourIndex);
        Assert.True(keys.Visible);
        Assert.True(keys.ShowLabels);
        var pin = keys.Notes[0];
        Assert.Equal((NoteKind.Pin, "Red key", 100, 200), (pin.Kind, pin.Label, pin.TileX, pin.TileY));
        Assert.Equal(Note.CategoryColour, pin.ColourIndex);
        var path = keys.Notes[1];
        Assert.Equal((NoteKind.Path, "Maze", 120, 200), (path.Kind, path.Label, path.TileX, path.TileY));
        Assert.Equal(
            [
                new PathSegment(120, 200, 120, 215), new PathSegment(120, 215, 135, 215),
                new PathSegment(135, 215, 135, 230),
            ],
            path.Segments);
    }

    [Theory]
    [InlineData("TLGMAP1:x", true)]
    [InlineData("TLGMAP2:x", true)]
    [InlineData("TLGMAP3:x", false)]
    [InlineData("TLGMAP", false)]
    [InlineData("", false)]
    public void HasPrefix_KnowsBothVersions(string text, bool hasPrefix)
    {
        Assert.Equal(hasPrefix, NoteShareCode.HasPrefix(text));
    }

    [Fact]
    public void ATreeCodeWithTwoTopCategories_IsDamaged()
    {
        var (set, _, _) = DungeonSet();
        var bytes = new List<byte>();
        NoteCodec.EncodeTree(set, 0, bytes);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(Code("TLGMAP2:", bytes.ToArray()), out _));
    }

    [Fact]
    public void ATreeCodeWithBytesLeftOver_IsDamaged()
    {
        var (set, dungeons, _) = DungeonSet();
        var bytes = new List<byte>();
        NoteCodec.EncodeTree(set, dungeons, bytes);
        bytes.Add(0);

        Assert.Equal(NoteCodeStatus.Damaged, NoteShareCode.Import(Code("TLGMAP2:", bytes.ToArray()), out _));
    }

    [Fact]
    public void ATreeCodeOver16MiB_IsTooLarge()
    {
        var zeros = new byte[16 * 1024 * 1024 + 1];

        Assert.Equal(NoteCodeStatus.TooLarge, NoteShareCode.Import(Code("TLGMAP2:", zeros), out _));
    }

    [Theory]
    [InlineData(262144, NoteCodeStatus.Damaged)]
    [InlineData(262145, NoteCodeStatus.TooLarge)]
    public void AFirstVersionCode_KeepsItsSmallerLimit(int size, NoteCodeStatus status)
    {
        Assert.Equal(status, NoteShareCode.Import(Code("TLGMAP1:", new byte[size]), out _));
    }

    [Fact]
    public void Describe_SummarisesTheImportAsTheCurrentClientDoes()
    {
        var (set, dungeons, _) = DungeonSet();
        NoteShareCode.Import(NoteShareCode.Export(set, dungeons)!, out var imported);

        Assert.Equal("Dungeons: 2 categories, 1 pins, 0 labels, 1 paths", NoteShareCode.Describe(imported!));
    }

    [Fact]
    public void Describe_SaysCategoryForOne()
    {
        NoteShareCode.Import("TLGMAP1:eNpj8U6tLGZiYmQPSk1RyE6tPME4gZmZxTexKpVZg4FBDowYGOQAuoUHpg==", out var imported);

        Assert.Equal("Keys: 1 category, 1 pins, 0 labels, 1 paths", NoteShareCode.Describe(imported!));
    }

    [Theory]
    [InlineData(NoteCodeStatus.NotACode, "That is not a world map code.")]
    [InlineData(NoteCodeStatus.Damaged, "This code is damaged or incomplete.")]
    [InlineData(NoteCodeStatus.TooLarge, "This code is too large to import.")]
    public void Problem_UsesTheCurrentClientsWords(NoteCodeStatus status, string message)
    {
        Assert.Equal(message, NoteShareCode.Problem(status));
    }

    [Fact]
    public void ExportMessages_CountTheCharactersAndWarnAboutDiscord()
    {
        Assert.Equal("Copied 1234 characters to the clipboard.", NoteShareCode.CopiedMessage(1234));
        Assert.Equal(
            "Too long for one Discord message; it will still work on the wiki.",
            NoteShareCode.TooLongForDiscord);
    }

    private static (NoteSet Set, int Dungeons, int Cavern) DungeonSet()
    {
        var set = new NoteSet();
        var dungeons = set.AddCategory("Dungeons", 5);
        var cavern = set.AddCategory("Cold Cavern", 1, dungeons);
        set.FindCategory(dungeons)!.ShowLabels = false;
        set.FindCategory(cavern)!.Visible = false;
        var pin = set.AddPoint(cavern, NoteKind.Pin, 300, 310, "Ice key");
        set.SetNoteColour(pin, 2);
        set.AddPath(cavern, [new PathSegment(100, 100, 100, 140), new PathSegment(100, 140, 130, 140)], "Way down");
        var towns = set.AddCategory("Towns", 3);
        set.AddPoint(towns, NoteKind.Pin, 700, 700, "Inn");
        return (set, dungeons, cavern);
    }

    private static List<byte> Header(string name, uint noteCount)
    {
        var bytes = new List<byte> { (byte)name.Length };
        bytes.AddRange(Encoding.UTF8.GetBytes(name));
        bytes.Add(0);
        NoteCodec.WriteVarint(bytes, noteCount);
        return bytes;
    }

    private static void Zigzag(List<byte> bytes, int value) =>
        NoteCodec.WriteVarint(bytes, (uint)((value << 1) ^ (value >> 31)));

    private static string FloodCode(int pathCount)
    {
        var bytes = Header("Flood", (uint)pathCount);
        for (var path = 0; path < pathCount; path++)
        {
            bytes.AddRange([(byte)NoteKind.Path, 0]);
            NoteCodec.WriteVarint(bytes, NoteLimits.SegmentsPerPath);
            for (var segment = 0; segment < NoteLimits.SegmentsPerPath; segment++)
            {
                Zigzag(bytes, segment == 0 && path == 0 ? 0 : -1);
                Zigzag(bytes, 0);
                Zigzag(bytes, 1);
                Zigzag(bytes, 0);
            }
        }

        return LegacyCode(bytes);
    }

    private static string LegacyCode(List<byte> category) => Code("TLGMAP1:", category.ToArray());

    private static string Code(string prefix, byte[] payload)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true))
        {
            zlib.Write(payload);
        }

        return prefix + Convert.ToBase64String(compressed.ToArray());
    }
}
