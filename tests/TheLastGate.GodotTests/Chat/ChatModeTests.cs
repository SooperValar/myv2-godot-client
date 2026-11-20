using GdUnit4;
using TheLastGate.Chat;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Chat;

[TestSuite]
public class ChatModeTests
{
    private ChatMode mode = new();

    [BeforeTest]
    public void StartWithTheDefaultMode() => mode = new ChatMode();

    [TestCase]
    public void WithoutEnterToTalk_TypingAndHotkeysBothWork()
    {
        AssertThat(mode.TakesTyping).IsTrue();
        AssertThat(mode.RunsHotkeys).IsTrue();
    }

    [TestCase(ChatKey.Enter)]
    [TestCase(ChatKey.Escape)]
    [TestCase(ChatKey.Slash)]
    [TestCase(ChatKey.Older)]
    [TestCase(ChatKey.Newer)]
    public void WithoutEnterToTalk_KeysAreLeftToTheChatLine(ChatKey key)
    {
        AssertThat(mode.Press(key)).IsEqual(ChatAction.None);
        AssertThat(mode.Active).IsFalse();
    }

    [TestCase]
    public void EnterToTalk_StartsClosedWithHotkeysAndNoTyping()
    {
        mode.EnterToTalk = true;

        AssertThat(mode.Active).IsFalse();
        AssertThat(mode.TakesTyping).IsFalse();
        AssertThat(mode.RunsHotkeys).IsTrue();
    }

    [TestCase(ChatKey.Enter, ChatAction.Open)]
    [TestCase(ChatKey.Slash, ChatAction.OpenWithSlash)]
    [TestCase(ChatKey.Older, ChatAction.OpenWithOlder)]
    public void WhileClosed_SomeKeysOpenTheChat(ChatKey key, ChatAction action)
    {
        mode.EnterToTalk = true;

        AssertThat(mode.Press(key)).IsEqual(action);

        AssertThat(mode.Active).IsTrue();
        AssertThat(mode.TakesTyping).IsTrue();
        AssertThat(mode.RunsHotkeys).IsFalse();
    }

    [TestCase(ChatKey.Escape)]
    [TestCase(ChatKey.Newer)]
    public void WhileClosed_EscapeAndNewerLeaveItClosed(ChatKey key)
    {
        mode.EnterToTalk = true;

        AssertThat(mode.Press(key)).IsEqual(ChatAction.None);
        AssertThat(mode.Active).IsFalse();
    }

    [TestCase(ChatKey.Enter, ChatAction.Send)]
    [TestCase(ChatKey.Escape, ChatAction.Cancel)]
    public void WhileOpen_EnterSendsAndEscapeCancelsThenItCloses(ChatKey key, ChatAction action)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        AssertThat(mode.Press(key)).IsEqual(action);

        AssertThat(mode.Active).IsFalse();
        AssertThat(mode.RunsHotkeys).IsTrue();
    }

    [TestCase(ChatKey.Slash)]
    [TestCase(ChatKey.Older)]
    [TestCase(ChatKey.Newer)]
    public void WhileOpen_OtherKeysAreLeftToTheChatLine(ChatKey key)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        AssertThat(mode.Press(key)).IsEqual(ChatAction.None);
        AssertThat(mode.Active).IsTrue();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ChangingTheOption_ClosesTheChat(bool enterToTalk)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        mode.EnterToTalk = enterToTalk;

        AssertThat(mode.Active).IsFalse();
    }
}
