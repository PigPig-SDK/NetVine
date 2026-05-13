using Core;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Notifications;

public class NotificationManager
{
    private static NotificationManager? _Instance;

    private static NotificationManager Instance
    {
        get => _Instance ??= Default;
        set => _Instance = value;
    }

    private static NotificationManager Default { get => new NotificationManager(); }

    private static readonly Dictionary<string, DateTime> _notificationCooldownTimer = [];

    public static event Action<Notification>? OnNotified;

    public static string DefaultFilePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine");
            Directory.CreateDirectory(folder);

            bool isMockSetup = Environment.GetCommandLineArgs().Contains(MockDataProducer.LaunchArgument);
            return Path.Combine(folder, isMockSetup ? "mocknotifications.json" : "notifications.json");
        }
    }
    [JsonInclude]
    private List<Notification> _notifications { get; set; } = [];
    public static IReadOnlyList<Notification> Notifications => Instance._notifications;

    public static int SpamCooldown { get; private set; } = 15;

    public static void WriteNotification(Notification notification)
    {
        Instance._notifications.Add(notification);
        OnNotified?.Invoke(notification);
    }

    public static void Initilaize()
    {
        try
        {
            Instance = LoadFromFile(DefaultFilePath);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            Debug.Log($"Failed to load config: {ex.Message}");
            Instance = Default;
            TrySaveToFile();
        }
    }
    private static NotificationManager LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<NotificationManager>(json) ?? Default;
    }
    private static void SaveToFile(string path, NotificationManager notificationManager)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(notificationManager, options);
        File.WriteAllText(path, json);
    }
    public static bool TrySaveToFile()
    {
        try
        {
            SaveToFile(DefaultFilePath, Instance);
            return true;
        }
        catch (IOException ex)
        {
            Debug.Log($"Failed to save config: {ex.Message}");
            return false;
        }
    }

    public static void ClearMessages()
    {
        Instance._notifications.Clear();
        TrySaveToFile();
    }

    public static void ProgramResourceNotification(ResourceTypes resource, ProgramData data)
    {
        string key = $"{resource} + {data}";
        if (!KeyCooldownMet(key)) return;

        WriteNotification(new Notification(NotificationPriority.Alert, $"{data.ProcessName} : Exceeded {resource}", $"{data.ProcessName} has been exceded in {resource} for this system!"));
    }
    public static void SystemResourceNotification(ResourceTypes resource)
    {
        string key = $"{resource}";
        if (!KeyCooldownMet(key)) return;
        WriteNotification(new Notification(NotificationPriority.Alert, $"System : Exceeded {resource}", $"{resource} total has been exceded for this sytem!"));
    }
    private static bool KeyCooldownMet(string key)
    {
        var now = DateTime.Now;

        if (_notificationCooldownTimer.TryGetValue(key, out var last))
        {
            if ((now - last).TotalSeconds <= SpamCooldown)
                return false;
            _notificationCooldownTimer[key] = now;
        }
        else
            _notificationCooldownTimer[key] = now;

        return true;
    }
}
