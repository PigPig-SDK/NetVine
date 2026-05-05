using Core;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public static void WriteNotification(Notification notification)
    {
        Instance._notifications.Add(notification);
        TrySaveToFile();
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
    private static bool TrySaveToFile()
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
}
