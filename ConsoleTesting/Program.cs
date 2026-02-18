
using Infrastructure;

WindowsDataProducer producer = new();

Console.WriteLine($"SYSNAME: {producer.SystemName}");

Console.WriteLine("Data here:");

foreach(var data in producer.Produce(1f))
{
    Console.WriteLine($"Process: {data.ProcessName}, Memory: {data.MemoryUsage} MB, CPU Time: {data.CpuUsage} seconds");
}

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

//Close reading...
producer.Dispose();
