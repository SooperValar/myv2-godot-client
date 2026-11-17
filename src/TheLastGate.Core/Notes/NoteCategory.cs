namespace TheLastGate.Notes;

public sealed class NoteCategory
{
    public int Id { get; internal set; }

    public int ParentId { get; internal set; }

    public string Name { get; internal set; } = "";

    public int ColourIndex { get; internal set; }

    public bool Visible { get; set; } = true;

    public bool ShowLabels { get; set; } = true;
}
