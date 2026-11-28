using GdUnit4;
using TheLastGate.Hud;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class HelpChatTests
{
    [TestCase("/help")]
    [TestCase("#HELP")]
    public void Request_OpensTheCommandsForHelp(string line)
    {
        AssertThat(HelpChat.Request(line)).IsEqual(new HelpRequest(HelpPageNumber.Commands));
    }

    [TestCase]
    public void Request_OpensTheQuests()
    {
        AssertThat(HelpChat.Request("/quest")).IsEqual(new HelpRequest(HelpPageNumber.Quests));
        AssertThat(HelpChat.Request("#quest all")).IsEqual(new HelpRequest(HelpPageNumber.Quests, AllQuests: true));
        AssertThat(HelpChat.Request("/quest 3")).IsEqual(new HelpRequest(HelpPageNumber.Quests, 3));
    }

    [TestCase("/areas", HelpPageNumber.Areas)]
    [TestCase("#kills", HelpPageNumber.KillQuests)]
    [TestCase("/Ranks", HelpPageNumber.Ranks)]
    [TestCase("/moddedarmor", HelpPageNumber.Armor)]
    [TestCase("/greataxe", HelpPageNumber.Weapons)]
    [TestCase("#rings", HelpPageNumber.Rings)]
    [TestCase("/belt", HelpPageNumber.Belts)]
    [TestCase("/specialw", HelpPageNumber.SpecialItems)]
    public void Request_OpensTheTabOfAListCommand(string line, HelpPageNumber tab)
    {
        AssertThat(HelpChat.Request(line)).IsEqual(new HelpRequest(tab));
    }

    [TestCase("/who")]
    [TestCase("/rank")]
    [TestCase("hello there")]
    [TestCase("/help me")]
    public void Request_LeavesOtherLinesToTheServer(string line)
    {
        AssertThat(HelpChat.Request(line)).IsNull();
    }
}
