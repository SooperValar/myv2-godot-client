using TheLastGate.Net;

namespace TheLastGate.Audio;

public static class ClickSound
{
    public static bool Plays(ClientCommand command) => command switch
    {
        ClientCommand.Input1 or ClientCommand.Input2 or ClientCommand.Input3 or ClientCommand.Input4
            or ClientCommand.Input5 or ClientCommand.Input6 or ClientCommand.Input7 or ClientCommand.Input8 => false,
        ClientCommand.AutoLook or ClientCommand.Meta or ClientCommand.TickReport
            or ClientCommand.PerformanceReport or ClientCommand.SetUser or ClientCommand.Unique => false,
        ClientCommand.NewLogin or ClientCommand.Login or ClientCommand.Challenge or ClientCommand.Password => false,
        _ => true,
    };
}
