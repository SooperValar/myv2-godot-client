namespace TheLastGate.Chat;

public enum ChatKey
{
    Enter,
    Escape,
    Slash,
    Older,
    Newer,
}

public enum ChatAction
{
    None,
    Open,
    OpenWithSlash,
    OpenWithOlder,
    Send,
    Cancel,
}

public sealed class ChatMode
{
    private bool enterToTalk;

    public bool EnterToTalk
    {
        get => enterToTalk;
        set
        {
            enterToTalk = value;
            Active = false;
        }
    }

    public bool Active { get; private set; }

    public bool TakesTyping => !EnterToTalk || Active;

    public bool RunsHotkeys => !EnterToTalk || !Active;

    public ChatAction Press(ChatKey key)
    {
        if (!EnterToTalk)
        {
            return ChatAction.None;
        }

        if (Active)
        {
            var whileOpen = OpenAction(key);
            Active = whileOpen == ChatAction.None;
            return whileOpen;
        }

        var whileClosed = ClosedAction(key);
        Active = whileClosed != ChatAction.None;
        return whileClosed;
    }

    private static ChatAction ClosedAction(ChatKey key) => key switch
    {
        ChatKey.Enter => ChatAction.Open,
        ChatKey.Slash => ChatAction.OpenWithSlash,
        ChatKey.Older => ChatAction.OpenWithOlder,
        _ => ChatAction.None,
    };

    private static ChatAction OpenAction(ChatKey key) => key switch
    {
        ChatKey.Enter => ChatAction.Send,
        ChatKey.Escape => ChatAction.Cancel,
        _ => ChatAction.None,
    };
}
