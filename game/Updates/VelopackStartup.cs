using System;
using System.Linq;
using Godot;
using Velopack;

namespace TheLastGate.Updates;

public static class VelopackStartup
{
    private const string HookPrefix = "--veloapp-";

    // The Starter answers Velopack's hooks. If one reaches Godot anyway, quit: Velopack's own exit after a hook
    // crashes a Godot process.
    public static bool HandledHook(SceneTree tree)
    {
        var arguments = OS.GetCmdlineArgs();
        if (arguments.Any(argument => argument.StartsWith(HookPrefix, StringComparison.Ordinal)))
        {
            tree.Quit();
            return true;
        }

        VelopackApp.Build().SetArgs(arguments).Run();
        return false;
    }
}
