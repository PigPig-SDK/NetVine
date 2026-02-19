
using Core;
using Infrastructure;

SystemHistory history = new(new WindowsDataProducer(), 10, TimeSpan.FromSeconds(1));

history.OnSnapshotTaken = (data) =>
{
    Console.WriteLine($"Snapshot taken at {DateTime.Now}");
    foreach (IProgramData item in data)
    {
        if(item.CpuUsage < 0.1f && item.DiskUsage < 0.1f && item.NetworkUsage < 0.1f)
            continue;
        Console.WriteLine($"{item.ProcessName} : CPU [{item.CpuUsage}%] NETWORK [{item.NetworkUsage} MB/S] RAM [{item.MemoryUsage}] Disc [{item.DiskUsage}]");
    }
};

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
