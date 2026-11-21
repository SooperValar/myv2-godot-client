using TheLastGate.State;

namespace TheLastGate.Game;

public sealed class GameSession
{
    private readonly GameState state;

    public GameSession(GameState state)
    {
        this.state = state;
        state.TickEnded += EndTheTick;
        state.TargetChanged += state.UseQueue.TargetChanged;
        state.HeldItemChanged += () => state.GiveMore.HeldItemChanged(state.Player);
        state.ShopOpened += _ => CloseTheWindowsAShopReplaces();
        state.ShopClosed += StartThePauseWhenAsked;
        state.WaypointsOffered += state.WaypointWindow.Open;
        state.WindowRequested += state.TextWindows.Show;
    }

    private void CloseTheWindowsAShopReplaces()
    {
        state.TextWindows.CloseAll();
        state.WaypointWindow.Close();
        state.SkillTreeWindow.Close();
    }

    private void StartThePauseWhenAsked(bool startsPause)
    {
        if (startsPause)
        {
            state.Pause.Start();
        }
    }

    private void EndTheTick()
    {
        if (state.Pause.Active)
        {
            state.ShutWindows();
        }

        state.Pause.EndOfTick();
        state.AutoWalk.EndOfTick(state.Map);
    }
}
