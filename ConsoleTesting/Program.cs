
using Core;
using Infrastructure;
using System.Diagnostics;

SystemHistory history = new(new WindowsDataProducer(),  TimeSpan.FromSeconds(1));
HashSet<string> trackedProcesses = new();
history.OnSnapshotTaken = (data) =>
{
    foreach (IProgramData item in data)
    {
        if(item is ProgramData mpc)
            if (trackedProcesses.Contains(item.ProcessName) || trackedProcesses.Contains("ALL"))
                Console.WriteLine($"{item.ProcessName} {mpc.ProcessId} : CPU [{item.CpuUsage}%] NETWORK [{item.NetworkUsage} MB/S] RAM [{item.MemoryUsage} MB] Disc [{item.DiskUsage} MB/S]");
    }
};

while (true)
{
    Console.WriteLine("Enter a program you want to track\n'clear' to clear you selection\n'list' to list programs" +
                      "\n'demo DB' to demo database; CAUTION will wipe DB");
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
    else if (input == "demo DB")
    {
        DBInteract.ClearAllProgramData();
        ProgramData testData = new ProgramData();
        testData.Date = DateTime.Now;
        
        Console.WriteLine("Enter system name");
        testData.SystemName = Console.ReadLine(); 
        
        Console.WriteLine("Enter program name");
        testData.ProcessName = Console.ReadLine();
        
        Console.WriteLine($"\nSubmitting...\n");
        DBInteract.SubmitEntry(testData);
        
        Console.WriteLine($"\nRetrieving...\n");
        DBInteract.ListAllToString();
        
        Console.WriteLine($"Deleting...\n");
        DBInteract.DeleteEntry(testData);
        DBInteract.ListAllToString();
        
        Console.WriteLine($"Populating Dummies...\n");
        DBInteract.PopulateDummyData();
        DBInteract.ListAllToString();
    }
    else if (input == "list")
    {
        foreach (Process process in Process.GetProcesses())
        {
            Console.WriteLine($"Name:'{process.ProcessName}' : ID:'{process.Id}'");
        }
    }
    else
    {
        trackedProcesses.Add(input);
        Console.WriteLine($"Added {input} to tracked processes");
    }
    
}
history.Dispose();