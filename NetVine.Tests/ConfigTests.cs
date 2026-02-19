using Infrastructure;

namespace NetVine.Tests;

/// <summary>
/// Todo: Implement tests for the ConfigManager
/// </summary>
public class ConfigTests
{
    [Fact]
    public void ReadFromFile_CreatesDefaultSettings_WhenFileNotFound()
    {
        var config = ConfigManager.Instance;
        bool result = config.ReadFromFile();

        Assert.False(result);
        Assert.NotNull(config.ConfigSettings);
        Assert.True(config.ConfigSettings.TrackCPUUsage);
    }
}
