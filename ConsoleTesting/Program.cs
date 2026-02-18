
using Infrastructure;

WindowsDataProducer producer = new();

Console.WriteLine($"SYSNAME: {producer.SystemName}");
Console.WriteLine("Data here:");

while (true)
{
    Thread.Sleep(10000);//10 second
    Console.WriteLine("10 seconds of data:");
    foreach (var data in producer.Produce(10.0f))
    {
        if (data.NetworkUsage == 0) continue;
        Console.WriteLine("----------------");
        Console.WriteLine($"PROC:{data.ProcessName},Net: {data.NetworkUsage} MB/s,CPU%: {data.CpuUsage}%");
    }
}

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

//Close reading...
producer.Dispose();
