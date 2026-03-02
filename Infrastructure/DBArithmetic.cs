using System.Linq;

namespace Infrastructure;

public static class DBArithmetic
{
    /// <summary>
    /// Find average cpu usage of database.
    /// </summary>
    /// <returns></returns> Average cpu usage.
    public static float AverageCpuUsage()
    {
        return DBInteract.ListAll().Average(x => x.CpuUsage);
    }
   
    /// <summary>
    /// Find average disk usage of database.
    /// </summary>
    /// <returns></returns> Average disk usage.
    public static float AverageDiskUsage()
    {
        return DBInteract.ListAll().Average(x => x.DiskUsage);
    }
    
    /// <summary>
    /// Find average network usage of database.
    /// </summary>
    /// <returns></returns> Average network usage.
    public static float AverageNetworkUsage()
    {
        return DBInteract.ListAll().Average(x => x.NetworkUsage);
    }
    
    /// <summary>
    /// Find average memory usage of database.
    /// </summary>
    /// <returns></returns> Average memory usage.
    public static float AverageMemoryUsage()
    {
        return DBInteract.ListAll().Average(x => x.MemoryUsage);
    }

    /// <summary>
    /// Find first instance of peak cpu usage of database.
    /// </summary>
    /// <returns></returns> Peak cpu usage.
    public static ProgramData PeakCpuUsage()
    {
        var maxCpuUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListAll().First(x => x.CpuUsage >= maxCpuUsage);
    }
    
    /// <summary>
    /// Find first instance of peak disk usage of database.
    /// </summary>
    /// <returns></returns> Peak disk usage.
    public static ProgramData PeakDisk()
    {
        var maxDiskUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListAll().First(x => x.CpuUsage >= maxDiskUsage);
    }
    
    /// <summary>
    /// Find first instance of peak network usage of database.
    /// </summary>
    /// <returns></returns> Peak network usage.
    public static ProgramData PeakNetworkUsage()
    {
        var maxNetworkUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListAll().First(x => x.CpuUsage >= maxNetworkUsage);
    }
    
    /// <summary>
    /// Find first instance of peak memory usage of database.
    /// </summary>
    /// <returns></returns> Peak memory usage.
    public static ProgramData PeakMemoryUsage()
    {
        var maxMemoryUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListAll().First(x => x.CpuUsage >= maxMemoryUsage);
    }

    /// <summary>
    /// Find the total amount of network that has been consumed across all entries.
    /// </summary>
    /// <returns></returns> Total netork usage.
    public static float TotalNetworkUsage()
    {
        return DBInteract.ListAll().Sum(x => x.NetworkUsage);
    }
    
}