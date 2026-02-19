using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Infrastructure;

/// <summary>
/// TODO: Fix filepath. Estimated time: 30 mins - 1 hr 
/// </summary>
public class ConfigManager
{
    /// <summary>
    /// Singleton lazy instance - thread safe
    /// </summary>
    private static readonly Lazy<ConfigManager> lazyInstance =
        new Lazy<ConfigManager>(() => new ConfigManager());


    /// <summary>
    /// Initializes a new instance of the Configuration class.
    /// </summary>
    private ConfigManager()
    {
        ConfigSettings = new Settings();


        filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NetVine",
            "config.yaml"
        );

        ReadFromFile();

    }

    public readonly string filePath;

    /// <summary>
    /// Gets the singleton instance of the Configuration class, providing access to application configuration settings.
    /// </summary>
    /// <remarks>This property ensures that the Configuration instance is created only once and is
    /// thread-safe. Access this property to retrieve configuration settings throughout the application.</remarks>
    public static ConfigManager Instance
    {
        get => lazyInstance.Value;
    }

    public Settings ConfigSettings { get; private set; }


    /// <summary>
    /// Provides configuration options for monitoring system resource usage and controlling performance data collection.
    /// </summary>
    /// <remarks>The Settings class enables users to specify which system resources to track, such as CPU,
    /// memory, network, and disk usage. It also allows configuration of the maximum storage limit for collected data
    /// and the interval at which resource usage is sampled. Adjust these settings to balance monitoring detail with
    /// storage and performance considerations.</remarks>
    public class Settings
    {

        public bool TrackCPUUsage { get; set; } = true;
        public bool TrackMemoryUsage { get; set; } = true;
        public bool TrackNetworkUsage { get; set; } = true;
        public bool TrackDiskUsage { get; set; } = true;
        public int StorageLimitMb { get; set; } = 1024;
        public float TickRate { get ; set ; } = 10.0f; // change default value to something reasonable. Not sure what units we're using quite yet

    }

    /// <summary>
    /// Reads data from a file and processes it according to the application's requirements.
    /// </summary>
    /// <remarks>This method does not take any parameters and does not return a value. Ensure that the file to
    /// be read is accessible and in the expected format to avoid runtime errors.</remarks>
    public bool ReadFromFile() 
    {
        try
        {
            var yamlFile = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder().Build();
            ConfigSettings = deserializer.Deserialize<Settings>(yamlFile);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: The file '{filePath}' was not found.");
            Console.WriteLine("Creating new YAML config file ... ");
            ConfigSettings = new Settings();

            try
            {
                WriteToFile();
            }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create config file: {ex.Message}");
            }

            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Error: Access denied when reading '{filePath}'. Check file permissions.");
            return false;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading the file: {ex.Message}");
            return false;
        }
        catch (YamlException ex)
        {
            Console.WriteLine($"Error: Failed to parse YAML in '{filePath}': {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return false;
        }

        return true;

    }

    /// <summary>
    /// Writes the current data to a file at a predefined location.
    /// </summary>
    /// <remarks>This method does not take any parameters and writes to a predefined file location. Ensure
    /// that the application has the necessary permissions to write to the specified location. The file may be
    /// overwritten if it already exists.</remarks>
    public void WriteToFile()
    {
        try
        {
            var serializer = new SerializerBuilder().Build();
            var yamlOutput = serializer.Serialize(ConfigSettings);
            File.WriteAllText(filePath, yamlOutput);
        }
        catch (YamlException ex)
        {
            throw new InvalidOperationException($"Failed to serialize configuration settings to YAML.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Access denied when writing to file: {filePath}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new InvalidOperationException($"Directory not found for file path: {filePath}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"An I/O error occurred while writing to file: {filePath}", ex);
        }
    }

}