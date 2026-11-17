using System.IO;
using Godot;
using TheLastGate.Net;
using TheLastGate.Notes;

namespace TheLastGate.Map;

// Owns the map notes for one server. The current client saves its notes straight after every change; whatever makes
// a change calls Save, and the game also saves when it ends.
public sealed class MapNotesKeeper
{
    private const string FilePrefix = "TLGAnnotations_";

    private readonly string ownPath;

    public MapNotesKeeper(ServerAddress server)
    {
        ownPath = ProjectSettings.GlobalizePath("user://" + ServerFiles.Name(FilePrefix, server.Host, server.Port));
        (Notes, WasSetAside) = NoteStore.Open(
            ownPath,
            ServerFiles.SdlClientPath(FilePrefix, server.Host, server.Port));
        if (WasSetAside)
        {
            GD.PushWarning($"The saved map notes couldn't be read and were set aside as {SetAsidePath}");
        }
    }

    public NoteSet Notes { get; }

    public bool WasSetAside { get; }

    public string SetAsidePath => ownPath + ".bad";

    public string FileName => Path.GetFileName(ownPath);

    public void Save()
    {
        if (Notes.Changed && !NoteStore.SaveTo(ownPath, Notes))
        {
            GD.PushWarning($"Couldn't save the map notes to {ownPath}");
        }
    }
}
