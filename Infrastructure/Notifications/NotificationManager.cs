using Core;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

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
            return Path.Combine(folder, isMockSetup ? "mocknotifications.yaml" : "notifications.yaml");
        }
    }
    [YamlMember]
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
        catch (Exception ex) when (ex is YamlException || ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            Debug.Log($"Failed to load config: {ex.Message}");
            Instance = Default;
            TrySaveToFile();
        }
    }
    private static NotificationManager LoadFromFile(string path)
    {
        var yamlFile = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new YamlNotificationConverter())
            .IncludeNonPublicProperties()
            .Build();
        var manager = deserializer.Deserialize<NotificationManager>(yamlFile);

        return manager;
    }
    private static void SaveToFile(string path, NotificationManager notificationManager)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlNotificationConverter())
            .IncludeNonPublicProperties()
            .Build();
        var yamlOutput = serializer.Serialize(notificationManager);
        File.WriteAllText(path, yamlOutput);
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
    private class YamlNotificationConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Notification);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (parser.Current is null) return new Notification();
            string value = ((Scalar)parser.Current).Value;
            parser.MoveNext();
            var split = value.Split(':');

            if (split.Length != 2)
                return new Notification();

            //Set connect info properly.
            NotificationPriority priority = (NotificationPriority)Enum.Parse(typeof(NotificationPriority), split[0], true);
            string message = split[1];
            return new Notification(priority, message);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var myType = (Notification)value!;

            emitter.Emit(new Scalar($"{myType.Priority}:{myType.Message}"));
        }
    }
}
