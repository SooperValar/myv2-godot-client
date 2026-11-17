using TheLastGate.Hud;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class LookPanelTests
{
    [Fact]
    public void Shown_IsEmptyBeforeAnyLook()
    {
        Assert.Null(new LookPanel().Shown);
    }

    [Fact]
    public void Hide_StopsShowingTheLookAtOnce()
    {
        var panel = new LookPanel();
        panel.Show(new CharacterLook());

        panel.Hide();

        Assert.Null(panel.Shown);
    }

    [Fact]
    public void Show_KeepsTheLookForTenSeconds()
    {
        var panel = new LookPanel();
        var look = new CharacterLook();
        panel.Show(look);

        panel.Advance(TimeSpan.FromSeconds(9.9));
        Assert.Same(look, panel.Shown);

        panel.Advance(TimeSpan.FromSeconds(0.1));
        Assert.Null(panel.Shown);
    }

    [Fact]
    public void Show_StartsTheTenSecondsAgainForTheNextLook()
    {
        var panel = new LookPanel();
        panel.Show(new CharacterLook());
        panel.Advance(TimeSpan.FromSeconds(8));
        var next = new CharacterLook();

        panel.Show(next);
        panel.Advance(TimeSpan.FromSeconds(8));

        Assert.Same(next, panel.Shown);
    }

    [Theory]
    [InlineData(LookFlags.None, SpriteTint.None)]
    [InlineData(LookFlags.Shop, SpriteTint.None)]
    [InlineData(LookFlags.Shadow, SpriteTint.Grey)]
    [InlineData(LookFlags.Bloody, SpriteTint.Infrared)]
    [InlineData(LookFlags.Shadow | LookFlags.Bloody, SpriteTint.Grey | SpriteTint.Infrared)]
    public void PortraitTint_GreysShadowsAndReddensTheBloody(LookFlags flags, SpriteTint tint)
    {
        Assert.Equal(tint, LookPanel.PortraitTint(flags));
    }
}
