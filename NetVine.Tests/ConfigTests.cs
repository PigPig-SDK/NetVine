using Infrastructure;
using YamlDotNet.Serialization;

namespace NetVine.Tests;

/// <summary>
/// TODO: Make these safer to run - make it so that currentFolder needs to be set in every test 
/// Delete once these are changed
///// </summary>
//public class ConfigTests
//{
//    private bool disposedValue;

//    string _CurrentFolder = "";
//    string _CurrentFilePath = "";

//    [Fact]
//    public void ReadWriteToFile_Test()
//    {
//        var i = ConfigManager.Instance;
//        ConfigManager.TrySaveToFile();
//        ConfigManager.TryLoadFromFile();

//        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
//        ConfigManager.WriteSetting(SettingInt.TrackNetworkUsage, 1);

//        Assert.Equivalent(1, ConfigManager.ReadSetting(SettingInt.TrackNetworkUsage));
//        Assert.Equivalent(0, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
//        Assert.Equivalent(1, ConfigManager.ReadSetting(SettingInt.TrackMemoryUsage));
//        Assert.Equivalent(1, ConfigManager.ReadSetting(SettingInt.TrackDiskUsage));
//        Assert.True(ConfigManager.ReadSetting(SettingFloat.TickRate) == 1000.0f);

//        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 1);


//        ConfigManager.TrySaveToFile();
//        ConfigManager.TryLoadFromFile();
        
//        Assert.Equivalent(1, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));

//    }

//    private void Cleanup()
//    {
//        if (Directory.Exists(_CurrentFolder))
//            Directory.Delete(_CurrentFolder, recursive: true);
//    }
//}

public class ConfigManagerTests : IDisposable
{
    private readonly string _tempFile;

    public ConfigManagerTests()
    {
        // Use a temp file for each test so they don't interfere with each other
        _tempFile = Path.GetTempFileName();
        ConfigManager.CustomFilePath = _tempFile;

        // Reset singleton between tests
        ResetInstance();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);

        ResetInstance();
        ConfigManager.CustomFilePath = null;
    }

    /// <summary>
    /// Resets the private singleton _Instance field via reflection so each test starts clean.
    /// </summary>
    private static void ResetInstance()
    {
        var field = typeof(ConfigManager).GetField("_Instance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field!.SetValue(null, null);
    }

    // -------------------------------------------------------------------------
    // Defaults
    // -------------------------------------------------------------------------

    [Fact]
    public void DefaultIntSettings_HaveExpectedValues()
    {
        Assert.Equal(1, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
        Assert.Equal(1, ConfigManager.ReadSetting(SettingInt.TrackDiskUsage));
        Assert.Equal(1, ConfigManager.ReadSetting(SettingInt.TrackMemoryUsage));
        Assert.Equal(1, ConfigManager.ReadSetting(SettingInt.TrackNetworkUsage));
    }

    [Fact]
    public void DefaultFloatSettings_HaveExpectedValues()
    {
        Assert.Equal(1000.0f, ConfigManager.ReadSetting(SettingFloat.TickRate));
    }

    [Fact]
    public void AllEnumValues_AreInitialized()
    {
        // Every SettingInt key should exist — ReadSetting should not throw
        foreach (var setting in Enum.GetValues<SettingInt>())
            ConfigManager.ReadSetting(setting);

        foreach (var setting in Enum.GetValues<SettingFloat>())
            ConfigManager.ReadSetting(setting);

        foreach (var setting in Enum.GetValues<SettingString>())
            ConfigManager.ReadSetting(setting);
    }

    // -------------------------------------------------------------------------
    // WriteSetting / ReadSetting
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteSetting_Int_PersistsValue()
    {
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
        Assert.Equal(0, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
    }

    [Fact]
    public void WriteSetting_Float_PersistsValue()
    {
        ConfigManager.WriteSetting(SettingFloat.TickRate, 500.0f);
        Assert.Equal(500.0f, ConfigManager.ReadSetting(SettingFloat.TickRate));
    }

    [Fact]
    public void WriteSetting_CanOverwriteMultipleTimes()
    {
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 1);
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
        Assert.Equal(0, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
    }

    // -------------------------------------------------------------------------
    // Singleton behaviour
    // -------------------------------------------------------------------------

    [Fact]
    public void Instance_ReturnsSameObject()
    {
        var a = ConfigManager.Instance;
        var b = ConfigManager.Instance;
        Assert.Same(a, b);
    }

    [Fact]
    public void WriteSetting_ReflectedOnInstance()
    {
        ConfigManager.WriteSetting(SettingInt.TrackDiskUsage, 0);
        Assert.Equal(0, ConfigManager.ReadSetting(SettingInt.TrackDiskUsage));
    }

    // -------------------------------------------------------------------------
    // TrySaveToFile / TryLoadFromFile round-trip
    // -------------------------------------------------------------------------

    [Fact]
    public void TrySaveToFile_ReturnsTrue_WhenPathIsValid()
    {
        Assert.True(ConfigManager.TrySaveToFile());
    }

    [Fact]
    public void TrySaveToFile_CreatesNonEmptyFile()
    {
        ConfigManager.TrySaveToFile();
        Assert.True(new FileInfo(_tempFile).Length > 0);
    }

    [Fact]
    public void RoundTrip_IntSetting_SurvivedSaveAndLoad()
    {
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
        ConfigManager.TrySaveToFile();

        ResetInstance();

        ConfigManager.TryLoadFromFile();
        Assert.Equal(0, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
    }

    [Fact]
    public void RoundTrip_FloatSetting_SurvivedSaveAndLoad()
    {
        ConfigManager.WriteSetting(SettingFloat.TickRate, 250.0f);
        ConfigManager.TrySaveToFile();

        ResetInstance();

        ConfigManager.TryLoadFromFile();
        Assert.Equal(250.0f, ConfigManager.ReadSetting(SettingFloat.TickRate));
    }

    [Fact]
    public void TryLoadFromFile_ReturnsTrue_WhenFileIsValid()
    {
        ConfigManager.TrySaveToFile();
        ResetInstance();
        Assert.True(ConfigManager.TryLoadFromFile());
    }

    // -------------------------------------------------------------------------
    // TryLoadFromFile error handling
    // -------------------------------------------------------------------------

    [Fact]
    public void TryLoadFromFile_ReturnsFalse_WhenFileIsMissing()
    {
        ConfigManager.CustomFilePath = "/nonexistent/path/config.yaml";
        ResetInstance();
        Assert.False(ConfigManager.TryLoadFromFile());
    }

    [Fact]
    public void TryLoadFromFile_ReturnsFalse_WhenFileIsCorrupt()
    {
        File.WriteAllText(_tempFile, "this: is: not: valid: yaml: !!!");
        ResetInstance();
        Assert.False(ConfigManager.TryLoadFromFile());
    }

    [Fact]
    public void TryLoadFromFile_FallsBackToDefaults_WhenFileIsMissing()
    {
        // Modify a setting, then point to a missing file so it falls back
        ConfigManager.WriteSetting(SettingInt.TrackCPUUsage, 0);
        ConfigManager.CustomFilePath = "/nonexistent/path/config.yaml";
        ResetInstance();

        ConfigManager.TryLoadFromFile();

        Assert.Equal(1, ConfigManager.ReadSetting(SettingInt.TrackCPUUsage));
    }

    [Fact]
    public void TrySaveToFile_ReturnsFalse_WhenPathIsInvalid()
    {
        ConfigManager.CustomFilePath = "/nonexistent/directory/config.yaml";
        Assert.False(ConfigManager.TrySaveToFile());
    }

    // -------------------------------------------------------------------------
    // ResolvedFilePath
    // -------------------------------------------------------------------------

    [Fact]
    public void ResolvedFilePath_UsesCustomPath_WhenSet()
    {
        ConfigManager.CustomFilePath = _tempFile;
        // Save should write to our temp file, not the default AppData path
        ConfigManager.TrySaveToFile();
        Assert.True(new FileInfo(_tempFile).Length > 0);
    }

    [Fact]
    public void ResolvedFilePath_UsesDefaultPath_WhenCustomPathIsNull()
    {
        ConfigManager.CustomFilePath = null;
        // TrySaveToFile should not throw — it falls back to the AppData path
        var result = ConfigManager.TrySaveToFile();
        Assert.True(result);
    }
}
