namespace TheLastGate.Hud;

// Name and SecondsLeft come from the server's buff timers; a slot it sent no timer for has neither.
public readonly record struct BuffIcon(int Sprite, float Brightness, string Name = "", int? SecondsLeft = null)
{
    public const int WarningSeconds = 10;

    private const int SecondsPerMinute = 60;
    private const int SecondsPerHour = 3600;

    public bool RunningOut => SecondsLeft < WarningSeconds;

    public string ShortTime => SecondsLeft switch
    {
        null => "",
        >= SecondsPerHour and var seconds => $"{seconds / SecondsPerHour}h",
        >= SecondsPerMinute and var seconds => $"{seconds / SecondsPerMinute}m",
        var seconds => $"{seconds}s",
    };

    public string LongTime => SecondsLeft switch
    {
        null => "",
        >= SecondsPerHour and var seconds =>
            $"{seconds / SecondsPerHour}:{seconds % SecondsPerHour / SecondsPerMinute:00}:{seconds % SecondsPerMinute:00} left",
        var seconds => $"{seconds / SecondsPerMinute}:{seconds % SecondsPerMinute:00} left",
    };
}
