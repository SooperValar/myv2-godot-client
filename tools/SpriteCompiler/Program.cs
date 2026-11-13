using TheLastGate.Assets;
using TheLastGate.Tools.SpriteCompiler;

const string usage = """
    Rebuilds the sprite archives from the source sprites.

    Usage: dotnet run --project tools/SpriteCompiler [<source folder> <archive folder>]

    With no arguments, compiles art/sprites into data/archives in this repository.
    """;

string sourceFolder;
string archiveFolder;
switch (args.Length)
{
    case 0:
        var repoRoot = FindRepoRoot();
        sourceFolder = Path.Combine(repoRoot, "art", "sprites");
        archiveFolder = Path.Combine(repoRoot, "data", DataLayout.Archives);
        break;
    case 2:
        sourceFolder = Path.GetFullPath(args[0]);
        archiveFolder = Path.GetFullPath(args[1]);
        break;
    default:
        Console.Error.WriteLine(usage);
        return 2;
}

Console.WriteLine($"Compiling sprites from {sourceFolder} into {archiveFolder} ...");
var report = SpriteCompiler.Compile(sourceFolder, archiveFolder);
(report.Succeeded ? Console.Out : Console.Error).Write(report.Format());
return report.Succeeded ? 0 : 1;

// The tool runs from its build output inside the repository, so the root is the nearest folder
// above it that holds the solution.
static string FindRepoRoot()
{
    for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
    {
        if (File.Exists(Path.Combine(folder.FullName, "TheLastGate.sln")))
        {
            return folder.FullName;
        }
    }

    throw new InvalidOperationException("TheLastGate.sln wasn't found above the sprite compiler's build folder.");
}
