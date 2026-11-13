namespace TheLastGate.Tests;

public sealed class TempFolder : IDisposable
{
    public TempFolder()
    {
        FolderPath = Path.Combine(Path.GetTempPath(), "tlg-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FolderPath);
    }

    public string FolderPath { get; }

    public void Dispose() => Directory.Delete(FolderPath, true);

    public string Write(string fileName, byte[] contents)
    {
        var filePath = Path.Combine(FolderPath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllBytes(filePath, contents);
        return filePath;
    }
}
