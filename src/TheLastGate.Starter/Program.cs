using System.Diagnostics;
using Velopack;

VelopackApp.Build().Run();

var installFolder = AppContext.BaseDirectory;
var gameExecutable = Path.Combine(
    installFolder,
    OperatingSystem.IsWindows() ? "TheLastGate.exe" : "TheLastGate.x86_64");
var start = new ProcessStartInfo(gameExecutable) { WorkingDirectory = installFolder, UseShellExecute = false };
foreach (var argument in args)
{
    start.ArgumentList.Add(argument);
}

var game = Process.Start(start);

// An AppImage is unmounted when its main process exits, which would pull the game's files out from under it.
if (!OperatingSystem.IsWindows())
{
    game?.WaitForExit();
}
