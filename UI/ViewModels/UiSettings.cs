using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModels;

public static class UiSettings
{
    /// <summary>
    /// This contains all settings for the application to display. Add options to this if you want them to appear as editable
    /// </summary>
    public static List<SettingGroup> AllSettings => new()
    {
        new SettingGroup("Application", [
            new SettingInputReporter($"Net-Vine version {Program.Version}",
                "This is your current netvine version.",
                "version", "app", "application"),


            new SettingInputCheckbox($"Start minimized",
                "Should netvine start minimized",
                SettingInt.StartMinimized,
                "load", "startup", "app", "application"),

            new SettingInputCheckbox($"Minimize on close",
                "When you close net-vine, should it remain silent in the background?",
                SettingInt.MinimizeOnClose,
                "minimize", "app", "application"),

            new SettingInputCheckbox($"Load on startup",
                "Should netvine start automatically with your OS?",
                SettingInt.LoadOnStartup,
                "load", "startup", "app", "application"),
            ]),


        new SettingGroup ("Network",[
            new SettingInputField<SettingInt>("Port for host",
                "The port used for hosting a server",
                StringInputMethod.IntInput,
                SettingInt.HostPort,
                "network", "host") { MaxAcceptedNumericalSize = 65535},

            new SettingInputField<SettingString>("Ip for host",
                "The ip used for hosting a server",
                StringInputMethod.StringInput,
                SettingString.HostIP,
                "network", "host"),

            new SettingInputField<SettingFloat>("Reconnect interval",
                "[In Seconds] How often the application attempts to re-connect to its hosts",
                StringInputMethod.FloatInput,
                SettingFloat.NetworkReconnectInterval,
                "network", "usage", "interval"),

            new SettingInputCheckbox("Disable Network Operations",
                "Disable all client connections and host connections. ",
                SettingInt.NetworkDisabled,
                "network", "usage"),
        ]),
        new SettingGroup ("Database Storage", [

            new SettingInputCheckbox("Track CPU usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackCPUUsage,
                "cpu", "usage", "db", "database"),

            new SettingInputCheckbox("Track memory usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackMemoryUsage,
                "ram", "usage", "db", "database"),

            new SettingInputCheckbox("Track network usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackNetworkUsage,
                "network", "usage", "db", "database"),

            new SettingInputCheckbox("Track disk usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackDiskUsage,
                "disk", "usage", "db", "database"),

            new SettingInputField<SettingFloat>("Database save interval",
                "[In Seconds] How often the application will write to your database.",
                StringInputMethod.FloatInput,
                SettingFloat.DatabaseSaveInterval,
                "database", "usage", "interval", "db", "database"),

            new SettingInputField<SettingFloat>("Probe rate",
                "[In Seconds] How often this application will probe all programs on your system. If you are experiencing overhead, try lowering this setting.",
                StringInputMethod.FloatInput,
                SettingFloat.TickRate,
                "memory", "usage", "interval", "db", "database"),
        ]),
    };

    public static SettingInput? GetInputField<T>(T inputType) where T : Enum
    {
        List<SettingGroup> settings = AllSettings;

        foreach (var settingsGroup in settings)
        {
            foreach (SettingInput setting in settingsGroup.Fields)
            {
                if(setting is null) continue;

                if(setting.GenericSetting is T tester)
                    if(tester.Equals(inputType)) return setting; 
            }
        }
        return null;
    }
}
