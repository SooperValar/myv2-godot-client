using TheLastGate.Chat;

namespace TheLastGate.Tests;

public class ChatModeTests
{
    private readonly ChatMode mode = new();

    [Fact]
    public void WithoutEnterToTalk_TypingAndHotkeysBothWork()
    {
        Assert.True(mode.TakesTyping);
        Assert.True(mode.RunsHotkeys);
    }

    [Theory]
    [InlineData(ChatKey.Enter)]
    [InlineData(ChatKey.Escape)]
    [InlineData(ChatKey.Slash)]
    [InlineData(ChatKey.Older)]
    [InlineData(ChatKey.Newer)]
    public void WithoutEnterToTalk_KeysAreLeftToTheChatLine(ChatKey key)
    {
        Assert.Equal(ChatAction.None, mode.Press(key));
        Assert.False(mode.Active);
    }

    [Fact]
    public void EnterToTalk_StartsClosedWithHotkeysAndNoTyping()
    {
        mode.EnterToTalk = true;

        Assert.False(mode.Active);
        Assert.False(mode.TakesTyping);
        Assert.True(mode.RunsHotkeys);
    }

    [Theory]
    [InlineData(ChatKey.Enter, ChatAction.Open)]
    [InlineData(ChatKey.Slash, ChatAction.OpenWithSlash)]
    [InlineData(ChatKey.Older, ChatAction.OpenWithOlder)]
    public void WhileClosed_SomeKeysOpenTheChat(ChatKey key, ChatAction action)
    {
        mode.EnterToTalk = true;

        Assert.Equal(action, mode.Press(key));

        Assert.True(mode.Active);
        Assert.True(mode.TakesTyping);
        Assert.False(mode.RunsHotkeys);
    }

    [Theory]
    [InlineData(ChatKey.Escape)]
    [InlineData(ChatKey.Newer)]
    public void WhileClosed_EscapeAndNewerLeaveItClosed(ChatKey key)
    {
        mode.EnterToTalk = true;

        Assert.Equal(ChatAction.None, mode.Press(key));
        Assert.False(mode.Active);
    }

    [Theory]
    [InlineData(ChatKey.Enter, ChatAction.Send)]
    [InlineData(ChatKey.Escape, ChatAction.Cancel)]
    public void WhileOpen_EnterSendsAndEscapeCancelsThenItCloses(ChatKey key, ChatAction action)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        Assert.Equal(action, mode.Press(key));

        Assert.False(mode.Active);
        Assert.True(mode.RunsHotkeys);
    }

    [Theory]
    [InlineData(ChatKey.Slash)]
    [InlineData(ChatKey.Older)]
    [InlineData(ChatKey.Newer)]
    public void WhileOpen_OtherKeysAreLeftToTheChatLine(ChatKey key)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        Assert.Equal(ChatAction.None, mode.Press(key));
        Assert.True(mode.Active);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ChangingTheOption_ClosesTheChat(bool enterToTalk)
    {
        mode.EnterToTalk = true;
        mode.Press(ChatKey.Enter);

        mode.EnterToTalk = enterToTalk;

        Assert.False(mode.Active);
    }
}
