using Core;
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
            new SettingInputReporter($"Net-Vine version {Netvine.Version}",
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
                65535,
                0,
                "network", "host"),

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

            new SettingInputCheckbox("Use network password",
                "Should a password be used? Incase privacy is not your thing.",
                SettingInt.UseNetworkPassword,
                "network", "host"),

            new SettingInputField<SettingString>("Host Password",
                "The password used for hosting",
                StringInputMethod.StringInput,
                SettingString.HostPassword,
                "network", "host"),

            new SettingInputCheckbox("Disable Network Operations",
                "Disable all client connections and host connections. ",
                SettingInt.NetworkDisabled,
                "network", "usage"),

            new SettingInputCheckbox("Allow host manipulation",
                "Allows the host to kill processes on your machine.",
                SettingInt.AllowHostManipulation,
                "network", "usage", "host", "manipulation", "close"),
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
                int.MaxValue,
                1,
                "database", "usage", "interval", "db", "database"),

            new SettingInputField<SettingFloat>("Probe rate",
                "[In Seconds] How often this application will probe all programs on your system. If you are experiencing overhead, try lowering this setting.",
                StringInputMethod.FloatInput,
                SettingFloat.TickRate,
                int.MaxValue,
                0.1m,
                "memory", "usage", "interval", "db", "database"),
        ]),
        new SettingGroup("Graph", [
            new SettingInputField<SettingInt>("Max history",
                "How many data points to keep in the graph history. At 1 poll/second, 60 = 60 seconds of history.",
                StringInputMethod.IntInput,
                SettingInt.MaxHistory,
                300,
                0,
                "graph", "history", "chart"),

            new SettingInputField<SettingInt>("Max chart listings",
                "How many processes to display on pie and bar charts.",
                StringInputMethod.IntInput,
                SettingInt.TopCount,
                20,
                0,
                "graph", "chart", "process", "pie", "bar"),
        ]),
        new SettingGroup("Notifications", [

            new SettingInputCheckbox("Disable Toast Notifications",
                "Sets weather the bottom right popups should occur when you get a notification.",
                SettingInt.DisableToastPopups,
                "notification"),

            new SettingInputField<SettingInt>("Notification Toast Timeout (Milliseconds)",
                "How long a toast notification should exist.",
                StringInputMethod.IntInput,
                SettingInt.NotificationTimeMS,
                100_000,
                0,
                "notification", "ram"),

            new SettingInputField<SettingFloat>("Application CPU Notification threshold (%)",
                "Set the CPU usage percentage at which you want to be notified about a process." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteIndividualCpuUsage,
                100,
                0,
                "notification", "cpu"),

            new SettingInputField<SettingFloat>("Application RAM Notification threshold (MB)",
                "Set the RAM usage at which you want to be notified about a process." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteIndividualRamUsage,
                (int)SystemHistory.Instance.GetTotalRam(),
                0,
                "notification", "ram"),

            new SettingInputField<SettingFloat>("Application Disk Notification threshold (MB/sec)",
                "Set the DISK usage at which you want to be notified about a process." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteIndividualDiskUsage,
                int.MaxValue,
                0,
                "notification", "disk"),

            new SettingInputField<SettingFloat>("Application Network Notification threshold (MB/sec)",
                "Set the NET usage at which you want to be notified about a process." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteIndividualNetworkUsage,
                int.MaxValue,
                0,
                "notification", "network"),

            new SettingInputField<SettingFloat>("Total CPU Notification threshold (%)",
                "Set the CPU usage percentage at which you want to be notified." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteCpuUsage,
                100,
                0,
                "notification", "cpu"),

            new SettingInputField<SettingFloat>("Application RAM Notification threshold (MB)",
                "Set the RAM usage at which you want to be notified." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteRamUsage,
                (int)SystemHistory.Instance.GetTotalRam(),
                0,
                "notification", "ram"),

            new SettingInputField<SettingFloat>("Application Disk Notification threshold (MB/sec)",
                "Set the DISK usage at which you want to be notified." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteDiskUsage,
                int.MaxValue,
                0,
                "notification", "disk"),

            new SettingInputField<SettingFloat>("Application Network Notification threshold (MB/sec)",
                "Set the NET usage at which you want to be notified." +
                "If a process exceeds this threshold, Netvine will send you an alert.",
                StringInputMethod.FloatInput,
                SettingFloat.NoteNetworkUsage,
                int.MaxValue,
                0,
                "notification", "network"),
        ]),
        new SettingGroup("API/Scripting", [

            new SettingInputCheckbox("Enable Python Script runner",
                "[Requires restart] Should the scripts folder be executed on netvine startup? Security warning, any python files in the scripting folder will be executed!",
                SettingInt.UsePythonScripting,
                "notification"),

            new SettingInputField<SettingString>("POST Notification Destination",
                "A URL for Net-Vine to make REST requests to. If left empty, no request is made.",
                StringInputMethod.StringInput,
                SettingString.ApiNotification,
                "api", "rest", "post"),

            new SettingInputField<SettingString>("POST DB-Snapshot Destination",
                "A URL for Net-Vine to make REST requests to. If left empty, no request is made.",
                StringInputMethod.StringInput,
                SettingString.ApiDataBaseSnapshot,
                "api", "rest", "post", "db"),

            new SettingInputField<SettingString>("POST Snapshot Destination",
                "A URL for Net-Vine to make REST requests to. If left empty, no request is made.",
                StringInputMethod.StringInput,
                SettingString.ApiSnapshot,
                "api", "rest", "post"),
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
