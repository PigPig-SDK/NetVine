using Core;
using System.Diagnostics;

namespace Infrastructure;

/// <summary>
/// Note: The mock data that comes from this might be unrealistic to obtain in a real setting!
/// </summary>
public class MockDataProducer : IProgramDataProducer
{
    public static string LaunchArgument = "-mock";

    public string SystemName => "Burger";

    public string[] RandomProcessNames = { "FrySource", "KetcHub", "Tar-Tar United", "KebabConnect"};

    private const double RamGigCount = 16;

    public MockDataProducer()
    {
        Core.Debug.Log("Mock data producer is being used!");
    }

    public void Dispose() { }

    public ICollection<IProgramData> Produce(TimeSpan rate)
    {
        List<IProgramData> result = [];
        Random random = new Random();

        foreach(string processName in  RandomProcessNames)
        {
            //Ketchub is our heavy load.
            if (processName == "KetcHub")
                result.Add(new ProgramData
                {
                    SystemName = SystemName,
                    Date = DateTime.Now,
                    ProcessName = processName,
                    CpuUsage = (float)(random.NextDouble() * 50),
                    DiskUsage = (float)(random.NextDouble() * 4080),
                    NetworkUsage = (float)(random.NextDouble() * 0.5f),
                    MemoryUsage = (float)(GetTotalRam() /3),
                });
            else
                result.Add(new ProgramData
                {
                    SystemName = SystemName,
                    Date = DateTime.Now,
                    ProcessName = processName,
                    CpuUsage = (float)(random.NextDouble()),
                    DiskUsage = (float)(random.NextDouble() * 20),
                    NetworkUsage = (float)(random.NextDouble() * 0.1f),
                    MemoryUsage = (float)(GetTotalRam()/7 + random.NextDouble()*50),
                });
        }

        return result;
    }

    public static bool IsBeingUsed()
    {
        return Environment.GetCommandLineArgs().Contains(LaunchArgument);
    }
    /// <summary>
    /// Total ram in MB
    /// </summary>
    public double GetTotalRam()
    {
        return RamGigCount * 1024;
    }

    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(string processName)
    {
        return (null, IconFileType.None);
    }

    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(Process process)
    {
        return (null, IconFileType.None);
    }
}
