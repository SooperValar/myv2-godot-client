using System.IO.Compression;
using System.Text;
using GdUnit4;
using TheLastGate.Notes;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Notes;

[TestSuite]
public class NoteShareCodeTests
{
    [TestCase]
    public void Export_WritesATreeCodeThatImportsBack()
    {
        var (set, dungeons, _) = DungeonSet();

        var code = NoteShareCode.Export(set, dungeons)!;

        AssertThat(code).StartsWith("TLGMAP2:");
        AssertThat(NoteShareCode.Import(code, out var imported)).IsEqual(NoteCodeStatus.Ok);
        AssertThat(imported!.Changed).IsFalse();
        AssertThat(imported.Tree().Select(category => category.Name)).ContainsExactly("Dungeons", "Cold Cavern");
        var root = imported.Tree()[0];
        var cavern = imported.Tree()[1];
        AssertThat(root.ParentId).IsEqual(0);
        AssertThat(root.ShowLabels).IsFalse();
        AssertThat(cavern.ParentId).IsEqual(root.Id);
        AssertThat(cavern.Visible).IsFalse();
        AssertThat(cavern.Notes.Count).IsEqual(2);
        AssertThat(cavern.Notes[0].ColourIndex).IsEqual(2);
    }

    [TestCase]
    public void Export_OfNoCategoryOrAnUnknownOne_GivesNothing()
    {
        var (set, _, _) = DungeonSet();

        AssertThat(NoteShareCode.Export(set, 0)).IsNull();
        AssertThat(NoteShareCode.Export(set, 9999)).IsNull();
    }

    [TestCase]
    public void ACodeBrokenUpWithSpacesAndLines_StillImports()
    {
        var (set, dungeons, _) = DungeonSet();
        var code = NoteShareCode.Export(set, dungeons)!;
        var spaced = " " + string.Join(" \n", code.Chunk(40).Select(part => new string(part))) + " ";

        AssertThat(NoteShareCode.Import(spaced, out var imported)).IsEqual(NoteCodeStatus.Ok);
        AssertThat(imported!.Categories.Count).IsEqual(2);
    }

    [TestCase("hello")]
    [TestCase("TLGMAP3:eNoDAAAAAAE=")]
    [TestCase("")]
    public void TextWithoutAKnownPrefix_IsNotACode(string text)
    {
        AssertThat(NoteShareCode.Import(text, out var imported)).IsEqual(NoteCodeStatus.NotACode);
        AssertThat(imported).IsNull();
    }

    [TestCase]
    public void ACodeCutShort_IsDamaged()
    {
        var (set, dungeons, _) = DungeonSet();
        var code = NoteShareCode.Export(set, dungeons)!;
        var shorter = code[..^8];

        AssertThat((shorter.Length - "TLGMAP2:".Length) % 4).IsEqual(0);
        AssertThat(NoteShareCode.Import(shorter, out _)).IsEqual(NoteCodeStatus.Damaged);
        AssertThat(NoteShareCode.Import(shorter[..^2], out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void ACoordinateOffTheMap_IsDamaged()
    {
        var category = Header("Bad", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 3000);
        Zigzag(category, 0);

        AssertThat(NoteShareCode.Import(LegacyCode(category), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void AHugeStep_IsDamaged()
    {
        var category = Header("Huge delta", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        NoteCodec.WriteVarint(category, 0xFFFFFFFE);
        Zigzag(category, 0);

        AssertThat(NoteShareCode.Import(LegacyCode(category), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void AHugeNoteCount_IsTooLarge()
    {
        AssertThat(NoteShareCode.Import(LegacyCode(Header("Huge", 100000)), out _)).IsEqual(NoteCodeStatus.TooLarge);
    }

    [TestCase]
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

        AssertThat(NoteShareCode.Export(set, guide)!.Length < NoteShareCode.DiscordLimit).IsTrue();
    }

    [TestCase]
    public void ACategoryOverItsSegmentLimit_IsTooLarge()
    {
        var pathsAtLimit = NoteLimits.SegmentsPerCategory / NoteLimits.SegmentsPerPath;

        AssertThat(NoteShareCode.Import(FloodCode(pathsAtLimit), out _)).IsEqual(NoteCodeStatus.Ok);
        AssertThat(NoteShareCode.Import(FloodCode(pathsAtLimit + 1), out _)).IsEqual(NoteCodeStatus.TooLarge);
    }

    [TestCase]
    public void ANamelessCategory_ImportsAsImported()
    {
        var category = Header("", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);

        AssertThat(NoteShareCode.Import(LegacyCode(category), out var imported)).IsEqual(NoteCodeStatus.Ok);
        AssertThat(imported!.Categories[0].Name).IsEqual("Imported");
        AssertThat(imported.Categories[0].Notes[0].Label).IsEqual("");
    }

    [TestCase]
    public void TextWithoutALabel_IsDamaged()
    {
        var category = Header("Notes", 1);
        category.AddRange([(byte)NoteKind.Text, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);

        AssertThat(NoteShareCode.Import(LegacyCode(category), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void BytesAfterTheCategory_AreDamaged()
    {
        var category = Header("Keys", 1);
        category.AddRange([(byte)NoteKind.Pin, 0]);
        Zigzag(category, 10);
        Zigzag(category, 20);
        category.Add(0);

        AssertThat(NoteShareCode.Import(LegacyCode(category), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void AFirstVersionCodeFromTheCurrentClient_StillImports()
    {
        const string code = "TLGMAP1:eNpj8U6tLGZiYmQPSk1RyE6tPME4gZmZxTexKpVZg4FBDowYGOQAuoUHpg==";

        AssertThat(NoteShareCode.Import(code, out var imported)).IsEqual(NoteCodeStatus.Ok);

        AssertThat(imported!.Categories).HasSize(1);
        var keys = imported.Categories[0];
        AssertThat(keys.Name).IsEqual("Keys");
        AssertThat(keys.ColourIndex).IsEqual(2);
        AssertThat(keys.Visible).IsTrue();
        AssertThat(keys.ShowLabels).IsTrue();
        var pin = keys.Notes[0];
        AssertThat((pin.Kind, pin.Label, pin.TileX, pin.TileY)).IsEqual((NoteKind.Pin, "Red key", 100, 200));
        AssertThat(pin.ColourIndex).IsEqual(Note.CategoryColour);
        var path = keys.Notes[1];
        AssertThat((path.Kind, path.Label, path.TileX, path.TileY)).IsEqual((NoteKind.Path, "Maze", 120, 200));
        AssertThat(path.Segments)
            .ContainsExactly(
                new PathSegment(120, 200, 120, 215),
                new PathSegment(120, 215, 135, 215),
                new PathSegment(135, 215, 135, 230));
    }

    [TestCase("TLGMAP1:x", true)]
    [TestCase("TLGMAP2:x", true)]
    [TestCase("TLGMAP3:x", false)]
    [TestCase("TLGMAP", false)]
    [TestCase("", false)]
    public void HasPrefix_KnowsBothVersions(string text, bool hasPrefix)
    {
        AssertThat(NoteShareCode.HasPrefix(text)).IsEqual(hasPrefix);
    }

    [TestCase]
    public void ATreeCodeWithTwoTopCategories_IsDamaged()
    {
        var (set, _, _) = DungeonSet();
        var bytes = new List<byte>();
        NoteCodec.EncodeTree(set, 0, bytes);

        AssertThat(NoteShareCode.Import(Code("TLGMAP2:", bytes.ToArray()), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void ATreeCodeWithBytesLeftOver_IsDamaged()
    {
        var (set, dungeons, _) = DungeonSet();
        var bytes = new List<byte>();
        NoteCodec.EncodeTree(set, dungeons, bytes);
        bytes.Add(0);

        AssertThat(NoteShareCode.Import(Code("TLGMAP2:", bytes.ToArray()), out _)).IsEqual(NoteCodeStatus.Damaged);
    }

    [TestCase]
    public void ATreeCodeOver16MiB_IsTooLarge()
    {
        var zeros = new byte[16 * 1024 * 1024 + 1];

        AssertThat(NoteShareCode.Import(Code("TLGMAP2:", zeros), out _)).IsEqual(NoteCodeStatus.TooLarge);
    }

    [TestCase(262144, NoteCodeStatus.Damaged)]
    [TestCase(262145, NoteCodeStatus.TooLarge)]
    public void AFirstVersionCode_KeepsItsSmallerLimit(int size, NoteCodeStatus status)
    {
        AssertThat(NoteShareCode.Import(Code("TLGMAP1:", new byte[size]), out _)).IsEqual(status);
    }

    [TestCase]
    public void Describe_SummarisesTheImportAsTheCurrentClientDoes()
    {
        var (set, dungeons, _) = DungeonSet();
        NoteShareCode.Import(NoteShareCode.Export(set, dungeons)!, out var imported);

        AssertThat(NoteShareCode.Describe(imported!)).IsEqual("Dungeons: 2 categories, 1 pins, 0 labels, 1 paths");
    }

    [TestCase]
    public void Describe_SaysCategoryForOne()
    {
        NoteShareCode.Import("TLGMAP1:eNpj8U6tLGZiYmQPSk1RyE6tPME4gZmZxTexKpVZg4FBDowYGOQAuoUHpg==", out var imported);

        AssertThat(NoteShareCode.Describe(imported!)).IsEqual("Keys: 1 category, 1 pins, 0 labels, 1 paths");
    }

    [TestCase(NoteCodeStatus.NotACode, "That is not a world map code.")]
    [TestCase(NoteCodeStatus.Damaged, "This code is damaged or incomplete.")]
    [TestCase(NoteCodeStatus.TooLarge, "This code is too large to import.")]
    public void Problem_UsesTheCurrentClientsWords(NoteCodeStatus status, string message)
    {
        AssertThat(NoteShareCode.Problem(status)).IsEqual(message);
    }

    [TestCase]
    public void ExportMessages_CountTheCharactersAndWarnAboutDiscord()
    {
        AssertThat(NoteShareCode.CopiedMessage(1234)).IsEqual("Copied 1234 characters to the clipboard.");
        AssertThat(NoteShareCode.TooLongForDiscord)
            .IsEqual("Too long for one Discord message; it will still work on the wiki.");
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
