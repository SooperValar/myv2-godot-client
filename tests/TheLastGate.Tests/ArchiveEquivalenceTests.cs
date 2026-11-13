using TheLastGate.Assets;
using TheLastGate.Tools.SpriteCompiler;

namespace TheLastGate.Tests;

// Compiles the real source sprites and checks the result is exactly what is committed in
// data/archives. This fails when art was added to art/sprites without rebuilding the archives.
public class ArchiveEquivalenceTests
{
    private static readonly string[] ArchiveFiles =
    [
        DataLayout.BmpIndexFile, DataLayout.BmpDataFile, DataLayout.PngIndexFile, DataLayout.PngDataFile,
    ];

    [Fact]
    public void CompiledArchives_MatchCommittedArchives()
    {
        var repoRoot = FindRepoRoot();
        using var output = new TempFolder();

        var report = SpriteCompiler.Compile(Path.Combine(repoRoot, "art", "sprites"), output.FolderPath);

        Assert.True(report.Succeeded, report.Format());
        foreach (var fileName in ArchiveFiles)
        {
            var committed = File.ReadAllBytes(Path.Combine(repoRoot, "data", DataLayout.Archives, fileName));
            var compiled = File.ReadAllBytes(Path.Combine(output.FolderPath, fileName));
            Assert.True(
                committed.AsSpan().SequenceEqual(compiled),
                $"{fileName} differs from a fresh compile of art/sprites. Run the sprite compiler and commit the archives.");
        }
    }

    private static string FindRepoRoot()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "TheLastGate.sln")))
            {
                return folder.FullName;
            }
        }

        throw new InvalidOperationException("TheLastGate.sln wasn't found above the test output folder.");
    }
}
