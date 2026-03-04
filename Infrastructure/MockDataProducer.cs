using Core;

namespace Infrastructure;

/// <summary>
/// Note: The mock data that comes from this might be unrealistic to obtain in a real setting!
/// </summary>
public class MockDataProducer : IProgramDataProducer
{
    public static string LaunchArgument = "-mock";

    public string SystemName => "Burger";

    public string[] RandomProcessNames = { "FrySource", "KetcHub", "Tar-Tar United", "KebabConnect"};

    public MockDataProducer()
    {
        Console.WriteLine("Mock data producer is being used!");
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
                    CpuUsage = (random.Next() * 50),
                    DiskUsage = (random.Next() * 4080),
                    NetworkUsage = (random.Next() * 0.5f),
                    MemoryUsage = (random.Next() * 25.0f),
                });
            else
                result.Add(new ProgramData
                {
                    SystemName = SystemName,
                    Date = DateTime.Now,
                    ProcessName = processName,
                    CpuUsage = (random.Next()),
                    DiskUsage = (random.Next() * 20),
                    NetworkUsage = (random.Next() * 0.1f),
                    MemoryUsage = (random.Next() * 10.0f),
                });
        }

        return result;
    }
}
