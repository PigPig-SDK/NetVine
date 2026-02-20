using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Infrastructure;

public enum Setting
{
    TrackDiskUsage,
    TrackCPUUsage,
    TrackMemoryUsage,
    TrackNetworkUsage,
    TickRate
}


/// <summary>
/// TODO: Fix filepath. Estimated time: 30 mins - 1 hr 
/// </summary>
public class ConfigManager
{

    /// <summary>
    /// Singleton instance
    /// </summary>
    [YamlIgnore]
    private static ConfigManager? _Instance;
    
    public Dictionary<Setting, float> FloatValues { get; set; }
    public Dictionary<Setting, string> StringValues { get; set; }
    public Dictionary<Setting, int> IntValues { get; set; }


    /// <summary>
    /// Constructor for The ConfigManager. Must remain public for deserialization when reading from the config file.
    /// </summary>
    public ConfigManager()
    {
        FloatValues = new Dictionary<Setting, float>();
        StringValues = new Dictionary<Setting, string>();
        IntValues = new Dictionary<Setting, int>();
    }


    /// <summary>
    /// Gets the default configuration manager instance with predefined settings for certain error cases.
    /// </summary>
    /// <remarks>This property initializes the configuration manager with default integer and float values for
    /// various system metrics. The default file path for configuration storage is also set during
    /// initialization.</remarks>
    [YamlIgnore]
    private static ConfigManager DefaultState
    {
        get
        {
            var manager = new ConfigManager();
            manager.IntValues = new Dictionary<Setting, int>()
            {
                [Setting.TrackDiskUsage] = 1,
                [Setting.TrackNetworkUsage] = 1,
                [Setting.TrackMemoryUsage] = 1,
                [Setting.TrackCPUUsage] = 1,
            };

            manager.FloatValues = new Dictionary<Setting, float>()
            {
                [Setting.TickRate] = 1000.0f
            };

            FilePath = GetDefaultFilePath();

            return manager;
        }

    }

    /// <summary>
    /// Config file path. 
    /// </summary>
    [YamlIgnore]
    private static string? _FilePath;

    [YamlIgnore]
    public static string FilePath
    {
        get
        {
            _FilePath ??= GetDefaultFilePath(); 
            return _FilePath;
        }
        set => _FilePath = value;
    }

    /// <summary>
    /// Gets the singleton instance of the configuration manager, initializing it to the default state if it has not
    /// already been created.
    /// </summary>
    /// <remarks>This property provides a global access point to the application's configuration manager. The
    /// instance is lazily initialized on first access. This property is thread-safe only if the underlying
    /// initialization logic is thread-safe. Use this property to retrieve or modify configuration settings throughout
    /// the application lifecycle.</remarks>
    [YamlIgnore]
    public static ConfigManager Instance
    {
        get
        {
            if (_Instance == null) _Instance = DefaultState;
            return _Instance;
        }
        private set => _Instance = value;
    }

    /// <summary>
    /// Resets the configuration file to its default state.
    /// </summary>
    /// <remarks>This method restores the configuration settings to their initial values and updates the
    /// configuration file accordingly. It is useful for reverting any changes made to the configuration during
    /// runtime.</remarks>
    public static void ResetConfFile()
    {
        Instance = DefaultState;
        UpdateConfFileWrite();
    }

    /// <summary>
    /// Gets the full path to the application's default configuration file within the user's application data directory.
    /// </summary>
    /// <remarks>If the NetVine directory does not exist, it is created automatically. This ensures a
    /// consistent and writable location for storing user-specific configuration data across different
    /// environments.</remarks>
    /// <returns>A string containing the full path to the configuration file, located in the NetVine subdirectory of the user's
    /// application data folder.</returns>
    private static string GetDefaultFilePath()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine");

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, "config.yaml");
    }


    /// <summary>
    /// Deserializes a YAML configuration file into a new instance of the ConfigManager class.
    /// </summary>
    /// <remarks>The method reads the entire content of the specified file and uses a YAML deserializer to
    /// convert it into a ConfigManager object. Ensure that the file exists and is accessible to avoid
    /// exceptions.</remarks>
    /// <param name="path">The path to the YAML file that contains the configuration data. This parameter must not be null or empty.</param>
    /// <returns>A ConfigManager instance populated with the data from the specified YAML file.</returns>
    private static ConfigManager GetFromFile(string path)
    {
        var yamlFile = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithNodeDeserializer(new EnumKeyDictionaryDeserializer(), s => s.InsteadOf<YamlDotNet.Serialization.NodeDeserializers.DictionaryNodeDeserializer>())
            .Build();
        return deserializer.Deserialize<ConfigManager>(yamlFile);
    }


    /// <summary>
    /// Serializes the specified configuration object to YAML format and writes the result to the specified file path.
    /// </summary>
    /// <remarks>If the specified file already exists, it will be overwritten. Ensure that the application has
    /// write permissions to the specified path.</remarks>
    /// <param name="path">The file path where the serialized YAML content will be saved. Cannot be null or empty.</param>
    /// <param name="config">The configuration object to serialize. This object contains the settings to be converted to YAML format.</param>
    private static void SendToFile(string path, ConfigManager config)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        var yamlOutput = serializer.Serialize(config);
        File.WriteAllText(path, yamlOutput);
    }

    /// <summary>
    /// Attempts to update the configuration file, reading it in from the file path. 
    /// If a yaml error, or a filenotfound error occurs, a new config file is created, and the exception messages are printed.
    /// </summary>
    /// <returns></returns>
    public static bool UpdateConfFileRead() 
    {
        Console.WriteLine($"pulling from {FilePath}");
        try
        {
            Instance = GetFromFile(FilePath);
        }
        catch (Exception ex) when (ex is YamlException || ex is FileNotFoundException)
        {
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Inner exception type: {ex.InnerException?.GetType().Name}");

            Instance = DefaultState;

            UpdateConfFileWrite();
            
        }
        return true;

    }

    /// <summary>
    /// Writes updaed settings to the config file.
    /// </summary>
    public static void UpdateConfFileWrite()
    {
        Console.WriteLine($"writing to {FilePath}");

        SendToFile(FilePath, Instance);

    }

    /// <summary>
    /// Makes it read the name of the Enum in the Yaml file instead of the enum's number value
    /// </summary>
    private class YamlStringEnumConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type.IsEnum;

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();
            return Enum.Parse(type, scalar.Value);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new Scalar(value!.ToString()!));
        }
    }

    /// <summary>
    /// Provides deserialization support for dictionaries with enum keys from a parser input.
    /// </summary>
    private class EnumKeyDictionaryDeserializer : INodeDeserializer
    {

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
        {

            if (expectedType.IsGenericType &&
               expectedType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
               expectedType.GetGenericArguments()[0].IsEnum)
            {
                var enumType = expectedType.GetGenericArguments()[0];
                var valueType = expectedType.GetGenericArguments()[1];
                var dict = (System.Collections.IDictionary)Activator.CreateInstance(expectedType)!;

                reader.Consume<MappingStart>();
                while (!reader.TryConsume<MappingEnd>(out _))
                {
                    var key = Enum.Parse(enumType, reader.Consume<Scalar>().Value);
                    var val = nestedObjectDeserializer(reader, valueType);
                    //Console.WriteLine($"key: {key}");
                    //Console.WriteLine($"val: {val}");
                    dict.Add(key, val);
                }

                value = dict;
                return true;
            }

            value = null;
            return false;
        }
    }

}