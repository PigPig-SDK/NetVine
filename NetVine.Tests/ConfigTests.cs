using Infrastructure;
using YamlDotNet.Serialization;

namespace NetVine.Tests;

/// <summary>
/// Todo: Implement tests for the ConfigManager
/// Delete once these are changed
/// </summary>
public class ConfigTests
{

    void Setup(ConfigSettings settings)
    {
        settings.TrackCPUUsage = true;
        settings.TrackMemoryUsage = true;
        settings.TrackDiskUsage = true;
        settings.TrackMemoryUsage = true;
        settings.TickRate = 1000;
    }

    [Fact]
    public void ReadFromFile_CreatesDefaultSettings_WhenFileNotFound()
    {
        var config = ConfigManager.Instance;
        bool result = config.ReadFromFile();

        Setup(config.ConfigSettings);

        Assert.True(result);
        Assert.NotNull(config.ConfigSettings);
        Assert.True(config.ConfigSettings.TrackCPUUsage);

        Assert.True(config.ConfigSettings.TrackCPUUsage == true);
        Assert.True(config.ConfigSettings.TrackMemoryUsage == true);
        Assert.True(config.ConfigSettings.TrackDiskUsage == true);
        Assert.True(config.ConfigSettings.TrackMemoryUsage == true);
        Assert.True(config.ConfigSettings.TickRate == 1000);
    }
}
