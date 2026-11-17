using TheLastGate.Audio;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ClickSoundTests
{
    [Theory]
    [InlineData(ClientCommand.Move)]
    [InlineData(ClientCommand.Attack)]
    [InlineData(ClientCommand.Use)]
    [InlineData(ClientCommand.PickUp)]
    [InlineData(ClientCommand.Drop)]
    [InlineData(ClientCommand.Give)]
    [InlineData(ClientCommand.Turn)]
    [InlineData(ClientCommand.Look)]
    [InlineData(ClientCommand.LookItem)]
    [InlineData(ClientCommand.Mode)]
    [InlineData(ClientCommand.Inventory)]
    [InlineData(ClientCommand.InventoryLook)]
    [InlineData(ClientCommand.Skill)]
    [InlineData(ClientCommand.Stat)]
    [InlineData(ClientCommand.Reset)]
    [InlineData(ClientCommand.Shop)]
    [InlineData(ClientCommand.QuickShop)]
    [InlineData(ClientCommand.Smith)]
    [InlineData(ClientCommand.Tree)]
    [InlineData(ClientCommand.Waypoints)]
    [InlineData(ClientCommand.MessageOfTheDay)]
    [InlineData(ClientCommand.Exit)]
    public void Plays_ForPlayerActions(ClientCommand command)
    {
        Assert.True(ClickSound.Plays(command));
    }

    [Theory]
    [InlineData(ClientCommand.Input1)]
    [InlineData(ClientCommand.Input2)]
    [InlineData(ClientCommand.Input3)]
    [InlineData(ClientCommand.Input4)]
    [InlineData(ClientCommand.Input5)]
    [InlineData(ClientCommand.Input6)]
    [InlineData(ClientCommand.Input7)]
    [InlineData(ClientCommand.Input8)]
    [InlineData(ClientCommand.AutoLook)]
    [InlineData(ClientCommand.Meta)]
    [InlineData(ClientCommand.TickReport)]
    [InlineData(ClientCommand.PerformanceReport)]
    [InlineData(ClientCommand.SetUser)]
    [InlineData(ClientCommand.Unique)]
    [InlineData(ClientCommand.NewLogin)]
    [InlineData(ClientCommand.Login)]
    [InlineData(ClientCommand.Challenge)]
    [InlineData(ClientCommand.Password)]
    public void Plays_NotForChatOrBackgroundRequests(ClientCommand command)
    {
        Assert.False(ClickSound.Plays(command));
    }
}
