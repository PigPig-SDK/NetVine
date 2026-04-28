
using Infrastructure;
using System.Diagnostics;

namespace Core;

public class LinuxDataProducer : IProgramDataProducer
{
    public string SystemName => "MR.Linux";

    public void Dispose()
    {

    }

    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(string processName)
    {
        return (null, IconFileType.None);
    }

    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(Process process)
    {
        return (null, IconFileType.None);
    }
    
    public double GetTotalRam()
    {
        string[] memInfo = File.ReadAllLines("/proc/meminfo");
        var totalMemLine = memInfo.FirstOrDefault(line => line.StartsWith("MemTotal:"));

        if (totalMemLine != null)
        {
            var parts = totalMemLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && long.TryParse(parts[1], out long memory))
                return memory * 1024;//KB to MB
        }
        return 0;
    }

    public ICollection<IProgramData> Produce(TimeSpan rate)
    {
        return Array.Empty<IProgramData>();
    }
}