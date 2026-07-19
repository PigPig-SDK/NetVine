using Core;
using Infrastructure;
using System.Reflection;

namespace Tests;

public class SystemHistoryTests
{
    public const int _totalRam = 10;
    public static ICollection<IProgramData> TestingProgramData() => [
                new ProgramData() { SystemName = "User", ProcessName = "Prog1", CpuUsage = 1, Timespan = 1,   MemoryUsage = 1, DiskUsage = 1, Date = DateTime.MinValue, NetworkUsage = 1, ProcessId = 0},
                new ProgramData() { SystemName = "User", ProcessName = "Prog2", CpuUsage = 1, Timespan = 1,   MemoryUsage = 1, DiskUsage = 1, Date = DateTime.MinValue, NetworkUsage = 1, ProcessId = 0},
                ];
    private class TestDataProducer : IProgramDataProducer
    {
        public string SystemName => "User";
        public float DataMultiplier { get; set; } = 1.0f; 

        public void Dispose()
        {
        }

        public (MemoryStream? image, IconFileType fileType) GetProcessIcon(string processName)
        {
            return (null, IconFileType.None);
        }
        public (MemoryStream? image, IconFileType fileType) GetProcessIcon(System.Diagnostics.Process process)
        {
            return GetProcessIcon(process.ProcessName);
        }

        public double GetTotalRam()
        {
            return _totalRam;
        }

        public ICollection<IProgramData> Produce(TimeSpan rate)
        {
            var data = TestingProgramData();
            foreach(IProgramData point in data)
            {
                point.CpuUsage *= DataMultiplier;
                point.NetworkUsage *= DataMultiplier;
                point.MemoryUsage *= DataMultiplier;
                point.DiskUsage *= DataMultiplier;
            }
            return data;
        }
    }

    private readonly TestDataProducer _producer;
    public SystemHistory SystemHistory { get; set; }

    public SystemHistoryTests()
    {
        
        _producer = new TestDataProducer();
        SystemHistory = new(_producer, TimeSpan.FromSeconds(1));
    }

    private MethodInfo GetIncrement()
    {
        return typeof(SystemHistory).GetMethod("TakeSnapshot", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("TakeSnapshot method not found via reflection.");
    }
    private void IncrementPoll()
    {
        var increment = GetIncrement();
        increment.Invoke(SystemHistory, new object?[] { null });
    }

    [Fact]
    public void GetLatestPoll_ReturnsData_Success()
    {
        //Assign
        //Do nothing here...
        //Act
        IncrementPoll();
        var poll = SystemHistory.GetLatestPoll();
        //Assert
        Assert.Equal(TestingProgramData(), poll);
    }
    [Fact]
    public void GetAverages_OneIncrement_Success()
    {
        //Assign
        //Do nothing here...
        //Act
        IncrementPoll();
        var averages = SystemHistory.GetAverages();
        foreach(var data in averages!) data.Date = DateTime.MinValue;
        //Assert
        Assert.Equal(TestingProgramData(), averages);
    }
    [Fact]
    public void GetAverages_TwoIncrements_Success()
    {
        //Assign
        //Act
        IncrementPoll();
        _producer.DataMultiplier = 2;
        IncrementPoll();
        var averages = SystemHistory.GetAverages();
        foreach (var data in averages!) data.Date = DateTime.MinValue;
        //Assert
        Assert.NotEqual(TestingProgramData(), averages);
        Assert.Equal(1, averages[0].Timespan);
        Assert.Equal(1, averages[1].Timespan);
        Assert.Equal(1.5, averages[0].CpuUsage);
        Assert.Equal(1.5, averages[1].CpuUsage);
    }
    [Fact]
    public void GetTotalRam_ReturnsCorrectValue()
    {
        Assert.Equal(_totalRam, SystemHistory.GetTotalRam());
    }
    [Fact]
    public void DatabaseSave_TwiceIncrement_FiresEvent()
    {
        //Assign
        ConfigManager.WriteSetting(SettingFloat.DatabaseSaveInterval, 2);
        int totalDBCalls = 0;
        DBInteract.OnProgramListAdded += (List<ProgramData> programs, bool isDataLocal)=> { totalDBCalls++; };
        //Act
        IncrementPoll();
        IncrementPoll();
        //Assert
        Assert.Equal(1, totalDBCalls);
    }
    [Fact]
    public void DatabaseSave_OnceIncrement_NoEvent()
    {
        //Assign
        ConfigManager.WriteSetting(SettingFloat.DatabaseSaveInterval, 2);
        int totalDBCalls = 0;
        DBInteract.OnProgramListAdded += (List<ProgramData> programs, bool isDataLocal) => { totalDBCalls++; };
        //Act
        IncrementPoll();
        //Assert
        Assert.Equal(0, totalDBCalls);
    }
    [Fact]
    public void OnSnapshotTaken_OnceIncrement_EventFired()
    {
        //Assign
        ConfigManager.WriteSetting(SettingFloat.DatabaseSaveInterval, 2);
        int calls = 0;
        List<IProgramData> output = [];
        SystemHistory.OnSnapshotTaken += (List<IProgramData> data) => { calls++; output = data; };
        //Act
        IncrementPoll();
        //Assert
        Assert.Equal(1, calls);
        Assert.Equal(output, TestingProgramData());
    }
}
