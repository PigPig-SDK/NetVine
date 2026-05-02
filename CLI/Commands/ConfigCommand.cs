using Infrastructure;
using System.Diagnostics;

namespace CLI.Commands;

public class ConfigCommand : Command
{
    public ConfigCommand(App app) : base(app) { }

    public override string Name => "configure";

    public override string Description => "Opens the configuration file in your explorer.";

    public override string Usage => "'configure'";

    public override void Execute(string[] args)
    {
        string configLocation = ConfigManager.DefaultFilePath;
        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(configLocation) { UseShellExecute = true });
        else if (OperatingSystem.IsMacOS())///Untested. Fuck mac.
            Process.Start("open", configLocation);
        else if(OperatingSystem.IsLinux())
        {
            Console.Write("\x1b[?1049l"); // Exit alternate screen buffer
            Process.Start("nano", configLocation)?.WaitForExit();
            Console.Write("\x1b[?1049h"); // Re-enter alternate screen buffer
        }

        App.SystemMessage($"Opened configuration file at '{configLocation}'");
    }
}
