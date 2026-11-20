using GdUnit4;
using TheLastGate.Audio;
using TheLastGate.Net;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Audio;

[TestSuite]
public class ClickSoundTests
{
    [TestCase(ClientCommand.Move)]
    [TestCase(ClientCommand.Attack)]
    [TestCase(ClientCommand.Use)]
    [TestCase(ClientCommand.PickUp)]
    [TestCase(ClientCommand.Drop)]
    [TestCase(ClientCommand.Give)]
    [TestCase(ClientCommand.Turn)]
    [TestCase(ClientCommand.Look)]
    [TestCase(ClientCommand.LookItem)]
    [TestCase(ClientCommand.Mode)]
    [TestCase(ClientCommand.Inventory)]
    [TestCase(ClientCommand.InventoryLook)]
    [TestCase(ClientCommand.Skill)]
    [TestCase(ClientCommand.Stat)]
    [TestCase(ClientCommand.Reset)]
    [TestCase(ClientCommand.Shop)]
    [TestCase(ClientCommand.QuickShop)]
    [TestCase(ClientCommand.Smith)]
    [TestCase(ClientCommand.Tree)]
    [TestCase(ClientCommand.Waypoints)]
    [TestCase(ClientCommand.MessageOfTheDay)]
    [TestCase(ClientCommand.Exit)]
    public void Plays_ForPlayerActions(ClientCommand command) =>
        AssertThat(ClickSound.Plays(command)).IsTrue();

    [TestCase(ClientCommand.Input1)]
    [TestCase(ClientCommand.Input2)]
    [TestCase(ClientCommand.Input3)]
    [TestCase(ClientCommand.Input4)]
    [TestCase(ClientCommand.Input5)]
    [TestCase(ClientCommand.Input6)]
    [TestCase(ClientCommand.Input7)]
    [TestCase(ClientCommand.Input8)]
    [TestCase(ClientCommand.AutoLook)]
    [TestCase(ClientCommand.Meta)]
    [TestCase(ClientCommand.TickReport)]
    [TestCase(ClientCommand.PerformanceReport)]
    [TestCase(ClientCommand.SetUser)]
    [TestCase(ClientCommand.Unique)]
    [TestCase(ClientCommand.NewLogin)]
    [TestCase(ClientCommand.Login)]
    [TestCase(ClientCommand.Challenge)]
    [TestCase(ClientCommand.Password)]
    public void Plays_NotForChatOrBackgroundRequests(ClientCommand command) =>
        AssertThat(ClickSound.Plays(command)).IsFalse();
}
