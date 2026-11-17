using System;
using TheLastGate.State;

namespace TheLastGate.Map;

public sealed class ExploredMapRecorder(ExploredMap map)
{
    public const int EmptyFloorSprite = 999;

    public bool Paused => throw new NotImplementedException();

    public ExploredMap Map => map;

    public static int? SpriteToAverage(uint flags, int floorSprite, int itemSprite) =>
        throw new NotImplementedException();

    public void See(int tileX, int tileY, uint colour) => throw new NotImplementedException();

    public void ClearBox(MinimapBox box) => throw new NotImplementedException();

    public void NoteLogLine(string line, int centreCharacter) => throw new NotImplementedException();

    public void NoteViewCentre(int centreCharacter) => throw new NotImplementedException();
}
