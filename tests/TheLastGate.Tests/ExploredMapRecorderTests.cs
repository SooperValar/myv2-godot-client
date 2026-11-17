using TheLastGate.Map;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class ExploredMapRecorderTests
{
    private const int Floor = 1100;
    private const int Item = 2200;
    private const int Own = 11;
    private const int Target = 22;

    private readonly ExploredMap map = new(64, 64);
    private readonly ExploredMapRecorder recorder;

    public ExploredMapRecorderTests() => recorder = new ExploredMapRecorder(map);

    [Fact]
    public void SpriteToAverage_PrefersTheItemOverTheFloor()
    {
        Assert.Equal(Item, ExploredMapRecorder.SpriteToAverage(0, Floor, Item));
        Assert.Equal(Floor, ExploredMapRecorder.SpriteToAverage(0, Floor, 0));
    }

    [Fact]
    public void SpriteToAverage_SkipsTilesThePlayerCanNotSee()
    {
        Assert.Null(ExploredMapRecorder.SpriteToAverage((uint)MapFlags.Invisible, Floor, Item));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ExploredMapRecorder.EmptyFloorSprite)]
    public void SpriteToAverage_SkipsUnlitAndOffMapTiles(int floor)
    {
        Assert.Null(ExploredMapRecorder.SpriteToAverage(0, floor, Item));
    }

    [Fact]
    public void See_RecordsTheColourAtTheTilesWorldPosition()
    {
        recorder.See(12, 34, 0x445566);

        Assert.Equal(0x445566u, map.ColourAt(12, 34));
    }

    [Fact]
    public void ClearBox_UnexploresTheBox()
    {
        recorder.See(5, 5, 0x0505);

        recorder.ClearBox(new MinimapBox(4, 4, 2, 2));

        Assert.Equal(0u, map.ColourAt(5, 5));
    }

    [Fact]
    public void WhileSpectating_NothingIsRecordedOrCleared()
    {
        recorder.See(5, 5, 0x0505);
        recorder.NoteLogLine("Now spectating Bob. Use /spectate self to return.", Own);

        recorder.See(6, 6, 0x0606);
        recorder.ClearBox(new MinimapBox(4, 4, 2, 2));

        Assert.True(recorder.Paused);
        Assert.Equal(0u, map.ColourAt(6, 6));
        Assert.Equal(0x0505u, map.ColourAt(5, 5));
    }

    [Fact]
    public void ComingBackToTheOwnCharacter_ResumesRecording()
    {
        recorder.NoteLogLine("Now spectating Bob. Use /spectate self to return.", Own);
        recorder.NoteViewCentre(Target);

        recorder.NoteViewCentre(Own);
        recorder.See(6, 6, 0x0606);

        Assert.False(recorder.Paused);
        Assert.Equal(0x0606u, map.ColourAt(6, 6));
    }
}
