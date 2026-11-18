using System;
using TheLastGate.Notes;

namespace TheLastGate.Map;

public static class MinimapOverlay
{
    public static void Paint(NoteSet? notes, Span<byte> rgba, int originX, int originY, int magnification) =>
        throw new NotImplementedException();

    public static Note? PinAt(NoteSet? notes, int originX, int originY, int magnification, int row, int column) =>
        throw new NotImplementedException();
}
