namespace TheLastGate.Tests;

public static class RepositoryFolder
{
    public static string Find()
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
