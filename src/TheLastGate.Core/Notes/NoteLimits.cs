namespace TheLastGate.Notes;

// The current client's limits; labels and names are counted in UTF-8 bytes, as its fixed buffers were.
public static class NoteLimits
{
    public const int LabelBytes = 47;
    public const int CategoryNameBytes = 31;
    public const int Categories = 256;
    public const int Depth = 8;
    public const int NotesPerCategory = 256;
    public const int SegmentsPerPath = 512;
    public const int SegmentsPerCategory = 4096;
    public const int Tiles = 2048;
}
