using Infrastructure;
using YamlDotNet.Serialization;

namespace NetVine.Tests;

/// <summary>
/// Todo: Implement tests for the ConfigManager
/// Delete once these are changed
/// </summary>
public class ConfigTests
{

    [Fact]
    public void ReadWriteToFile_Test()
    {
        ConfigManager.ResetConfFile();
        ConfigManager.UpdateConfFileRead();
        ConfigManager.UpdateConfFileWrite();

        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackCPUUsage]);
        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackNetworkUsage]);
        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackDiskUsage]);
        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackMemoryUsage]);

        Assert.True(ConfigManager.Instance.FloatValues[Setting.TickRate] == 1000.0f);

        ConfigManager.Instance.IntValues[Setting.TrackCPUUsage] = 0;

        ConfigManager.UpdateConfFileWrite();
        ConfigManager.UpdateConfFileRead();



        Assert.Equivalent(0, ConfigManager.Instance.IntValues[Setting.TrackCPUUsage]);


    }


}
