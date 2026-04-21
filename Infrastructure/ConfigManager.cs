using Infrastructure.Networking;
using NetCoreServer;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure;


public class ConfigManager
{

    private static ConfigManager? _Instance;

    private static ConfigManager Instance
    {
        get => _Instance ??= _Default;
        set => _Instance = value;
    }

    /// <summary>
    /// Gets the default instance of the ConfigManager class, which provides access to configuration settings.
    /// </summary>
    /// <remarks>This property creates a new instance of ConfigManager each time it is accessed. It is
    /// intended for scenarios where a single instance is not maintained, ensuring that configuration settings are
    /// retrieved fresh with each call.</remarks>
    private static ConfigManager _Default { get => new ConfigManager(); }

    /// <summary>
    /// Gets the default file path for the application's configuration file in the user's application data folder.
    /// </summary>
    /// <remarks>The path is constructed by combining the application data directory with a subdirectory named
    /// 'NetVine' and the file name 'config.yaml'. The directory is created if it does not already exist.</remarks>
    private static string _DefaultFilePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine");
            Directory.CreateDirectory(folder);

            bool isMockSetup = Environment.GetCommandLineArgs().Contains(MockDataProducer.LaunchArgument);
            return Path.Combine(folder,isMockSetup ? "mockconfig.yaml" :"config.yaml");
        }
    }
    /// <summary>Dictionary containing default integer setting values.</summary>
    private static readonly Dictionary<SettingInt, int> _DefaultIntValues = new()
    {
        {SettingInt.TrackDiskUsage, 1},
        {SettingInt.TrackCPUUsage, 1},
        {SettingInt.TrackMemoryUsage, 1},
        {SettingInt.TrackNetworkUsage,  1},
        {SettingInt.IsHosting, 0},
        {SettingInt.LastActivePage, 0 },
        {SettingInt.WindowWidth, 700 },
        {SettingInt.WindowHeight, 500 },
        {SettingInt.HostPort, NetworkManager.DefaultPort},
        {SettingInt.MinimizeOnClose, 0 },
        {SettingInt.LoadOnStartup, 0 },
        {SettingInt.StartMinimized, 0 },
        {SettingInt.NetworkDisabled, 0 },
        {SettingInt.MaxHistory, 100 },
        {SettingInt.TopCount, 10 }
    };


    /// <summary>Dictionary containing default float setting values.</summary>
    private static readonly Dictionary<SettingFloat, float> _DefaultFloatValues = new()
    {
        {SettingFloat.TickRate, 1.0f },//1 second
        {SettingFloat.DatabaseSaveInterval, 60.0f },//60 seconds
        {SettingFloat.NetworkReconnectInterval, 15.0f }//15 seconds!
    };


    /// <summary>Dictionary containing default string setting values.</summary>
    private static readonly Dictionary<SettingString, string> _DefaultStringValues = new()
    {
        { SettingString.HostIP, NetworkManager.DefaultHost}
    };
    
    
    /// <summary></summary>
    public static string? CustomFilePath {  get; set; }
    
    
    /// <summary>Custom Config file path. If null, the default value is used.</summary>
    public static string ResolvedFilePath => CustomFilePath ?? _DefaultFilePath;


    /// <summary>
    /// Stores integer settings identified by unique keys of type SettingInt.
    /// </summary>
    /// <remarks>This dictionary is intended for internal use within the class to manage application settings
    /// that require integer values. It should not be accessed directly from outside the containing class.</remarks>
    [YamlMember]
    private Dictionary<SettingInt, int> _IntSettings { get; set; }
    [YamlMember]
    private Dictionary<SettingFloat, float> _FloatSettings { get; set; }
    [YamlMember]
    private Dictionary<SettingString, string> _StringSettings { get; set; }
    [YamlMember]
    private List<ConnectionInfo> _ClientConnections { get; set; }

    public static IReadOnlyList<ConnectionInfo> ClientConnections => Instance._ClientConnections;

    /// <summary>
    /// Called when a setting is changed.
    /// The parameter is the enum of the setting.
    /// </summary>
    /// <remarks>Expect Enum of : SettingInt, SettingFloat or SettingString</remarks>
    public static Action<Enum>? OnSettingChanged { get; set; }
    public static Action<ConnectionInfo>? OnClientConnectionAdded { get; set; }
    public static Action<ConnectionInfo>? OnClientConnectionRemoved { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConfigManager class with default configuration file path and empty settings
    /// dictionaries.
    /// </summary>
    /// <remarks>After instantiation, settings are initialized to their default values. To load or modify
    /// settings, call the appropriate methods provided by the class. This constructor does not read from any
    /// configuration files automatically.</remarks>
    public ConfigManager() 
    {
        _IntSettings = [];
        _FloatSettings = [];
        _StringSettings = [];
        _ClientConnections = [];
        InitializeDefaults();
    }

    /// <summary>
    /// Initialization function. Called after CustomPath is set (or not set)
    /// Loads the file from disk.
    /// </summary>
    public static void Initialize()
    {
        TryLoadFromFile();
    }

    /// <summary>
    /// Initializes the internal dictionaries that store integer, float, and string settings with their respective
    /// default values.
    /// </summary>
    /// <remarks>This method ensures that each defined setting is present as a key in the corresponding
    /// dictionary. If a default value for a setting is not specified, a predefined fallback value is assigned. This
    /// guarantees that all settings are initialized and available for subsequent operations.</remarks>
    private void InitializeDefaults()
    {
        foreach (var setting in Enum.GetValues<SettingInt>())
            _IntSettings.TryAdd(setting, _DefaultIntValues.GetValueOrDefault(setting));

        foreach (var setting in Enum.GetValues<SettingFloat>())
            _FloatSettings.TryAdd(setting, _DefaultFloatValues.GetValueOrDefault(setting));

        foreach (var setting in Enum.GetValues<SettingString>())
            _StringSettings.TryAdd(setting, _DefaultStringValues.GetValueOrDefault(setting, string.Empty));
    }

    /// <summary>
    /// Retrieves the value associated with the specified application setting.
    /// </summary>
    /// <remarks>An exception may be thrown if the specified setting is not valid. Ensure that the provided
    /// setting exists in the configuration before calling this method.</remarks>
    /// <param name="setting">The setting for which to retrieve the integer value. Must be a valid member of the SettingInt enumeration.</param>
    /// <returns>The integer value corresponding to the specified setting.</returns>
    public static int ReadSetting(SettingInt setting) => Instance._IntSettings[setting];
    public static float ReadSetting(SettingFloat setting) => Instance._FloatSettings[setting];
    public static string ReadSetting(SettingString setting) => Instance._StringSettings[setting];
    public static bool ReadSettingBool(SettingInt setting) => Instance._IntSettings[setting] == 1;

    /// <summary>
    /// Assigns the specified integer value to the given integer setting.
    /// </summary>
    /// <remarks>This method updates the internal storage for integer settings. Ensure that the provided
    /// setting is valid before calling this method.</remarks>
    /// <param name="setting">The integer setting to update. Must be a valid value of the SettingInt enumeration.</param>
    /// <param name="value">The integer value to assign to the specified setting.</param>
    public static void WriteSetting(SettingInt setting, int value) 
    {
        Instance._IntSettings[setting] = value;
        OnSettingChanged?.Invoke(setting);
    }

    /// <summary>
    /// Assigns the specified float value to the given float setting.
    /// </summary>
    public static void WriteSetting(SettingFloat setting, float value) 
    {  
        Instance._FloatSettings[setting] = value;
        OnSettingChanged?.Invoke(setting);
    }

    /// <summary>
    /// Assigns the specified string value to the given string setting.
    /// </summary>
    public static void WriteSetting(SettingString setting, string value) 
    {  
        Instance._StringSettings[setting] = value;
        OnSettingChanged?.Invoke(setting);
    }
    /// <summary>
    /// Adds a ip to the list of connections
    /// </summary>
    public static void AddClientConnection(ConnectionInfo connectionInfo)
    {
        if (Instance._ClientConnections.Contains(connectionInfo))
            return;//Don't add duplicate.

        Instance._ClientConnections.Add(connectionInfo);
        OnClientConnectionAdded?.Invoke(connectionInfo);
    }
    /// <summary>
    /// Removes a connection from the configuration
    /// </summary>
    public static void RemoveClientConnection(ConnectionInfo connectionInfo)
    {
        if(Instance._ClientConnections.Remove(connectionInfo))
        {
            OnClientConnectionRemoved?.Invoke(connectionInfo);
        }
    }
    /// <summary>
    /// Loads a configuration from a YAML file at the specified path.
    /// </summary>
    /// <remarks>Ensure that the file at the specified path is in a valid YAML format. This method will throw
    /// an exception if the file cannot be read or if the content is not valid YAML.</remarks>
    /// <param name="path">The path to the YAML file containing the configuration. This parameter cannot be null or empty.</param>
    /// <returns>An instance of the ConfigManager class populated with the data from the YAML file.</returns>
    private static ConfigManager LoadFromFile(string path)
    {
        var yamlFile = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithTypeConverter(new YamlConnectionInfoConverter())
            .IncludeNonPublicProperties()
            .Build();
        var manager = deserializer.Deserialize<ConfigManager>(yamlFile);
        manager.InitializeDefaults();

        return manager;
    }


    /// <summary>
    /// Serializes the specified configuration object to YAML format and writes it to the given file path.
    /// </summary>
    /// <remarks>This method uses the YamlDotNet library for serialization. Ensure that the specified path is
    /// accessible and writable before calling this method.</remarks>
    /// <param name="path">The file path where the YAML content will be saved. Cannot be null or empty.</param>
    /// <param name="config">The configuration object to serialize. Cannot be null.</param>
    private static void SaveToFile(string path, ConfigManager config)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithTypeConverter(new YamlConnectionInfoConverter())
            .IncludeNonPublicProperties()
            .Build();
        var yamlOutput = serializer.Serialize(config);
        File.WriteAllText(path, yamlOutput);
    }

    /// <summary>
    /// Attempts to load the configuration from the specified file. If loading fails due to a missing file or a YAML
    /// parsing error, the method falls back to a default configuration.
    /// </summary>
    /// <remarks>When an error occurs during loading, diagnostic information is written to the console, and
    /// the default configuration is saved to the file. This method does not throw exceptions for file not found or YAML
    /// parsing errors; instead, it handles them internally and returns false.</remarks>
    /// <returns>true if the configuration was successfully loaded from the file; otherwise, false.</returns>
    public static bool TryLoadFromFile()
    {
        Console.WriteLine($"pulling from {ResolvedFilePath}");
        try
        {
            Instance = LoadFromFile(ResolvedFilePath);
            return true;
        }
        catch (Exception ex) when (ex is YamlException || ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Inner exception type: {ex.InnerException?.GetType().Name}");
            Console.WriteLine("Falling back to default configuration ... ");

            Instance = _Default;
            TrySaveToFile();

            return false;
        }

    }


    /// <summary>
    /// Attempts to save the current configuration to a file using the default file path if none is specified.
    /// </summary>
    /// <remarks>Handles any IO exceptions that occur during the save operation and logs an error message if
    /// the save fails.</remarks>
    /// <returns>true if the configuration is successfully saved; otherwise, false.</returns>
    public static bool TrySaveToFile()
    {
        Console.WriteLine($"writing to {ResolvedFilePath}");
        try
        {
            SaveToFile(ResolvedFilePath, Instance);
            return true;
        }
        catch(IOException ex)
        {
            Console.WriteLine($"Failed to save config: {ex.Message}");
            return false;
        }

    }

    /// <summary>
    /// Makes it write the name of the Enum in the Yaml file instead of the enum's number value
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

    private class YamlConnectionInfoConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(ConnectionInfo);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            ConnectionInfo connectionInfo = new("ParseFailure", 404);
            string value = ((Scalar)parser.Current).Value;
            parser.MoveNext();
            var split = value.Split(':');

            if (split.Length != 2)
                return connectionInfo;

            //Set connect info properly.
            connectionInfo.Ip = split[0];
            int.TryParse(split[1], out int expectedPort);
            connectionInfo.Port = expectedPort;
            return connectionInfo;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var myType = (ConnectionInfo)value!;

            emitter.Emit(new Scalar($"{myType.Ip}:{myType.Port}"));
        }
    }
}
