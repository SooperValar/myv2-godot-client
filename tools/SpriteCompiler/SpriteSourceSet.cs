namespace TheLastGate.Tools.SpriteCompiler;

// Paths are full paths, keyed by sprite number.
public sealed record SpriteSourceSet(
    SortedDictionary<int, string> Bitmaps,
    SortedDictionary<int, string> Pngs,
    List<SkippedFile> Skipped,
    List<int> InBothFormats);
