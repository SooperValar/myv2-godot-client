using System;
using System.IO;
using System.Linq;
using Velopack.Sources;

namespace TheLastGate.Updates;

public static class Updaters
{
    private const string PreviewArgument = "--preview-updates";
    private const string FailingPreviewArgument = "--preview-updates=fail-download";
    private const string FeedFolderArgument = "--update-feed=";

    public static IUpdater Choose(string[] userArguments)
    {
        if (userArguments.Any(argument => argument.StartsWith(PreviewArgument, StringComparison.Ordinal)))
        {
            return new PreviewUpdater(userArguments.Contains(FailingPreviewArgument));
        }

        var feedFolder = userArguments.FirstOrDefault(argument =>
            argument.StartsWith(FeedFolderArgument, StringComparison.Ordinal));
        if (feedFolder is not null)
        {
            return new VelopackUpdater(
                new SimpleFileSource(new DirectoryInfo(feedFolder[FeedFolderArgument.Length..])));
        }

        return new VelopackUpdater(new GithubReleaseSource(new HttpClientFileDownloader()));
    }
}
