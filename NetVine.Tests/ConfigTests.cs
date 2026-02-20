using Infrastructure;
using YamlDotNet.Serialization;

namespace NetVine.Tests;

/// <summary>
/// TODO: Make these safer to run - make it so that currentFolder needs to be set in every test 
/// Delete once these are changed
/// </summary>
public class ConfigTests
{
    private bool disposedValue;

    string _CurrentFolder = "";
    string _CurrentFilePath = "";

    [Fact]
    public void ReadWriteToFile_Test()
    {
        _CurrentFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine");
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
        Cleanup();
    }

    [Fact]
    public void MoveFilePath_Test()
    {

        Cleanup();

        ConfigManager.ResetConfFile();
        //set folder to temp data
        var folder = Path.Combine(Path.GetTempPath(), "NetVine");
        _CurrentFolder = folder;
        Cleanup();

        Directory.CreateDirectory(folder);
        ConfigManager.FilePath = Path.Combine(folder, "config.yaml");

        ConfigManager.UpdateConfFileWrite();
        ConfigManager.UpdateConfFileRead();

        ConfigManager.Instance.IntValues[Setting.TrackNetworkUsage] = 0;

        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackCPUUsage]);
        Assert.Equivalent(0, ConfigManager.Instance.IntValues[Setting.TrackNetworkUsage]);
        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackDiskUsage]);
        Assert.Equivalent(1, ConfigManager.Instance.IntValues[Setting.TrackMemoryUsage]);
        Assert.True(ConfigManager.Instance.FloatValues[Setting.TickRate] == 1000.0f);

        ConfigManager.Instance.IntValues[Setting.TrackCPUUsage] = 0;

        ConfigManager.UpdateConfFileWrite();
        ConfigManager.UpdateConfFileRead();
        
        Assert.Equivalent(0, ConfigManager.Instance.IntValues[Setting.TrackCPUUsage]);

        Cleanup();
    }

    private void Cleanup()
    {
        if (Directory.Exists(_CurrentFolder))
            Directory.Delete(_CurrentFolder, recursive: true);
    }
}
