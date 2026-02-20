
using Core;
using Infrastructure;

SystemHistory history = new(new WindowsDataProducer(), 10, TimeSpan.FromSeconds(1));
HashSet<string> trackedProcesses = new();
history.OnSnapshotTaken = (data) =>
{
    foreach (IProgramData item in data)
    {
        if(item is MockProgramDataClass mpc)
            if (trackedProcesses.Contains(item.ProcessName))
                Console.WriteLine($"{item.ProcessName} {mpc.ProcessId} : CPU [{item.CpuUsage}%] NETWORK [{item.NetworkUsage} MB/S] RAM [{item.MemoryUsage} MB] Disc [{item.DiskUsage} bytes]");
    }
};

while (true)
{
    Console.WriteLine("Enter a program you want to track");
    string? input = Console.ReadLine();
    if(string.IsNullOrEmpty(input))
    {
        Console.WriteLine("Invalid input, try again");
        continue;
    }
    else if(input == "exit")
    {
        history.Dispose();
        break;
    }
    else if(input == "clear")
    {
        trackedProcesses.Clear();
        Console.WriteLine("Cleared tracked processes");
    }
    else
    {
        trackedProcesses.Add(input);
        Console.WriteLine($"Added {input} to tracked processes");
    }
    
}
history.Dispose();