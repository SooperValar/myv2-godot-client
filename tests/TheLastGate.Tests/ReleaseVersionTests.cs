using System.Reflection;
using System.Text.Json;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ReleaseVersionTests
{
    private const string VersionFile = "Directory.Build.props";
    private const string ReleasePleaseVersionMarker = "x-release-please-version";

    [Fact]
    public void BuiltClientVersion_MatchesTheReleasePleaseManifest()
    {
        var manifestVersion = ReadJson(".release-please-manifest.json").GetProperty(".").GetString();
        var builtVersion = typeof(Protocol).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split('+')[0];

        Assert.Equal(manifestVersion, builtVersion);
    }

    [Fact]
    public void ReleasePlease_RaisesTheVersionLineTheBuildReads()
    {
        var extraFiles = ReadJson("release-please-config.json")
            .GetProperty("packages")
            .GetProperty(".")
            .GetProperty("extra-files")
            .EnumerateArray()
            .Select(file => file.GetString());
        var versionLine = Assert.Single(
            File.ReadLines(Path.Combine(RepositoryFolder.Find(), VersionFile)),
            line => line.Contains("<Version>"));

        Assert.Contains(VersionFile, extraFiles);
        Assert.Contains(ReleasePleaseVersionMarker, versionLine);
    }

    private static JsonElement ReadJson(string fileName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryFolder.Find(), fileName)));
        return document.RootElement.Clone();
    }
}
