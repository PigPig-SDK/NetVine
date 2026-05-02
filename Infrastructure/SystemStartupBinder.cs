
using Core;
using Microsoft.Win32;
using System.Reflection;

namespace Infrastructure;

public class SystemStartupBinder
{
    public static void Setup()
    {
        ExecuteConfiguration();
        ConfigManager.OnSettingChanged += OnSettingUpdated;
    }

    private static void OnSettingUpdated(Enum setting)
    {
        if(setting is not SettingInt settingInt) return;
        if (settingInt != SettingInt.LoadOnStartup) return;
        //Only do this if the setting is LoadOnStartup
        ExecuteConfiguration();
    }
    
    private static void ExecuteConfiguration()
    {
        if(OperatingSystem.IsWindows())
        {
            WindowsBinding();
        }
        else
        {
            Debug.Log("No specific configuration for this operating system, using defaults.");
        }
    }

    private static void WindowsBinding()
    {
        if (!OperatingSystem.IsWindows()) return;

        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);

        string exePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
        
        Debug.Log("Configuring startup settings for Windows. Executable path: " + exePath); 

        if (key is null)
        {
            Debug.Log("Failed to open registry key for startup configuration.");
            return;
        }

        if (ConfigManager.ReadSettingBool(SettingInt.LoadOnStartup))
            key?.SetValue(Netvine.AppName, $"\"{exePath}\"");
        else
            key?.DeleteValue(Netvine.AppName, throwOnMissingValue: false);

    }

}
