using System.Text;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

public class SpriteCompilerTests
{
    private static readonly string[] ArchiveFiles = ["bmplib.idx", "bmplib.dat", "pnglib.idx", "pnglib.dat"];

    [Fact]
    public void Compile_WritesAllFourArchives()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00010.bmp", TestBitmaps.Create(2, 1, (_, _) => (255, 0, 0)));
        folder.Write("sprites/00020.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 255)));
        var png = TestPngs.Create(4, 4, 3);
        folder.Write("sprites/18001.png", png);

        var report = Compile(folder);

        Assert.True(report.Succeeded, report.Error);
        Assert.Equal(2, report.BmpSpriteCount);
        Assert.Equal(1, report.PngSpriteCount);
        Assert.Empty(Directory.GetFiles(ArchiveFolder(folder), "*.tmp"));

        var bmpIndex = ReadArchive(folder, "bmplib.idx");
        Assert.Equal((2, 1, 0), TestArchives.BmpEntry(bmpIndex, 10));
        Assert.Equal((1, 1, 4), TestArchives.BmpEntry(bmpIndex, 20));
        Assert.Equal(new byte[] { 0x00, 0xF8, 0x00, 0xF8, 0x1F, 0x00 }, ReadArchive(folder, "bmplib.dat"));

        Assert.Equal(4, TestArchives.PngOffset(ReadArchive(folder, "pnglib.idx"), 18001));
        Assert.Equal([.. "SCAR"u8.ToArray(), .. png], ReadArchive(folder, "pnglib.dat"));
    }

    [Fact]
    public void Compile_ReportsRejectedBitmapsAndKeepsGoing()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00331.bmp", TestBitmaps.Create(1001, 1, (_, _) => (1, 2, 3)));
        folder.Write("sprites/00332.bmp", TestBitmaps.Create(2, 2, (_, _) => (1, 2, 3)));

        var report = Compile(folder);

        Assert.True(report.Succeeded, report.Error);
        Assert.Equal(1, report.BmpSpriteCount);
        var skipped = Assert.Single(report.Skipped);
        Assert.Equal("00331.bmp", skipped.FileName);
        Assert.Equal("size 1001x1 is larger than 1000x1000", skipped.Reason);
        var bmpIndex = ReadArchive(folder, "bmplib.idx");
        Assert.Equal((0, 0, 0), TestArchives.BmpEntry(bmpIndex, 331));
        Assert.Equal((2, 2, 0), TestArchives.BmpEntry(bmpIndex, 332));
    }

    [Fact]
    public void Compile_SkipsPngsThatDoNotDecode()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00001.png", Encoding.ASCII.GetBytes("not really a png"));
        var whole = TestPngs.Create(4, 4, 5);
        folder.Write("sprites/00002.png", whole[..^20]);
        folder.Write("sprites/00003.png", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        folder.Write("sprites/00004.png", whole);

        var report = Compile(folder);

        Assert.True(report.Succeeded, report.Error);
        Assert.Equal(1, report.PngSpriteCount);
        Assert.Equal(
            new[] { "00001.png", "00002.png", "00003.png" },
            report.Skipped.Select(skipped => skipped.FileName));
    }

    // Skia decodes a PNG with no IEND as long as its image data is whole, so the compiler checks for it.
    [Fact]
    public void Compile_SkipsPngWithoutEndChunk()
    {
        using var folder = new TempFolder();
        var whole = TestPngs.Create(4, 4, 5);
        folder.Write("sprites/00001.png", whole[..^12]);
        folder.Write("sprites/00002.png", whole);

        var report = Compile(folder);

        Assert.True(report.Succeeded, report.Error);
        Assert.Equal("00001.png", Assert.Single(report.Skipped).FileName);
    }

    [Fact]
    public void Compile_ReportsBothFormats()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00791.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        folder.Write("sprites/00791.png", TestPngs.Create(1, 1, 1));

        var report = Compile(folder);

        Assert.True(report.Succeeded, report.Error);
        Assert.Equal(new[] { 791 }, report.InBothFormats);
        Assert.Equal((1, 1, 0), TestArchives.BmpEntry(ReadArchive(folder, "bmplib.idx"), 791));
        Assert.Equal(4, TestArchives.PngOffset(ReadArchive(folder, "pnglib.idx"), 791));
    }

    [Fact]
    public void Compile_FailsWhenNoSourcesFound()
    {
        using var folder = new TempFolder();
        Directory.CreateDirectory(Path.Combine(folder.FolderPath, "sprites"));
        var before = WriteExistingArchives(folder);

        var report = Compile(folder);

        Assert.False(report.Succeeded);
        AssertArchivesUnchanged(folder, before);
    }

    [Fact]
    public void Compile_FailsWithoutThrowingWhenSourceAndArchiveFoldersAreMissing()
    {
        using var folder = new TempFolder();

        var report = Compile(folder);

        Assert.False(report.Succeeded);
        Assert.Contains("doesn't exist", report.Error);
    }

    [Fact]
    public void Compile_FailsWhenEveryBitmapIsRejected()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00001.bmp", TestBitmaps.Create(1001, 1, (_, _) => (0, 0, 0)));
        folder.Write("sprites/00002.bmp", Encoding.ASCII.GetBytes("not a bmp"));
        folder.Write("sprites/00003.png", TestPngs.Create(1, 1, 1));
        var before = WriteExistingArchives(folder);

        var report = Compile(folder);

        Assert.False(report.Succeeded);
        Assert.Contains("bmplib", report.Error);
        AssertArchivesUnchanged(folder, before);
    }

    [Fact]
    public void Compile_FailsWhenEveryPngIsRejected()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00001.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        folder.Write("sprites/00002.png", Encoding.ASCII.GetBytes("not a png"));
        var before = WriteExistingArchives(folder);

        var report = Compile(folder);

        Assert.False(report.Succeeded);
        Assert.Contains("pnglib", report.Error);
        AssertArchivesUnchanged(folder, before);
    }

    [Fact]
    public void Compile_FailsOnLfsPointerFiles()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00010.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        folder.Write(
            "sprites/00011.bmp",
            Encoding.ASCII.GetBytes("version https://git-lfs.github.com/spec/v1\noid sha256:ab\nsize 6198\n"));
        var before = WriteExistingArchives(folder);

        var report = Compile(folder);

        Assert.False(report.Succeeded);
        Assert.Contains("git lfs pull", report.Error);
        AssertArchivesUnchanged(folder, before);
    }

    [Fact]
    public void Compile_FailsCleanlyWhenTargetIsLocked()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00010.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        var before = WriteExistingArchives(folder);

        CompileReport report;
        var lockedPath = Path.Combine(ArchiveFolder(folder), "pnglib.dat");
        using (new FileStream(lockedPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            report = Compile(folder);
        }

        Assert.False(report.Succeeded);
        Assert.Contains("close the client", report.Error);
        AssertArchivesUnchanged(folder, before);
    }

    [Fact]
    public void Compile_ResultIsTheSameEveryRun()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00010.bmp", TestBitmaps.Create(3, 2, (column, row) => ((byte)column, (byte)row, 9)));
        folder.Write("sprites/00500.png", TestPngs.Create(2, 2, 1));
        folder.Write("sprites/00007.png", TestPngs.Create(3, 3, 2));

        Assert.True(Compile(folder).Succeeded);
        var first = ReadArchives(folder);
        Assert.True(Compile(folder).Succeeded);

        AssertArchivesUnchanged(folder, first);
    }

    [Fact]
    public void Format_ListsCountsSkipsAndBothFormats()
    {
        using var folder = new TempFolder();
        folder.Write("sprites/00791.bmp", TestBitmaps.Create(1, 1, (_, _) => (0, 0, 0)));
        folder.Write("sprites/00791.png", TestPngs.Create(1, 1, 1));
        folder.Write("sprites/Thumbs.db", [1]);

        var text = Compile(folder).Format();

        Assert.Contains("bmplib: 1 sprites", text);
        Assert.Contains("pnglib: 1 sprites", text);
        Assert.Contains("Thumbs.db", text);
        Assert.Contains("791", text);
    }

    private static CompileReport Compile(TempFolder folder) =>
        SpriteCompiler.Compile(Path.Combine(folder.FolderPath, "sprites"), ArchiveFolder(folder));

    private static string ArchiveFolder(TempFolder folder) => Path.Combine(folder.FolderPath, "archives");

    private static byte[] ReadArchive(TempFolder folder, string fileName) =>
        File.ReadAllBytes(Path.Combine(ArchiveFolder(folder), fileName));

    private static Dictionary<string, byte[]> WriteExistingArchives(TempFolder folder)
    {
        foreach (var fileName in ArchiveFiles)
        {
            folder.Write(Path.Combine("archives", fileName), Encoding.ASCII.GetBytes("old " + fileName));
        }

        return ReadArchives(folder);
    }

    private static Dictionary<string, byte[]> ReadArchives(TempFolder folder) =>
        ArchiveFiles.ToDictionary(fileName => fileName, fileName => ReadArchive(folder, fileName));

    private static void AssertArchivesUnchanged(TempFolder folder, Dictionary<string, byte[]> before)
    {
        foreach (var (fileName, contents) in before)
        {
            Assert.Equal(contents, ReadArchive(folder, fileName));
        }

        Assert.Empty(Directory.GetFiles(ArchiveFolder(folder), "*.tmp"));
    }
}
