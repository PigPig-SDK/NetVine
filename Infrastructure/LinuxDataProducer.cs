
using Infrastructure;
using System.Diagnostics;

namespace Core;

/// /mnt/c/users/djpig/documents/github/netvine/CLI
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
        return 0;
    }

    public ICollection<IProgramData> Produce(TimeSpan rate)
    {
        return Array.Empty<IProgramData>();
    }
}