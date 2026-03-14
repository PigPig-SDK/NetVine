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
        return DBInteract.ListAllProgramData().Average(x => x.CpuUsage);
    }
   
    /// <summary>
    /// Find average disk usage of database.
    /// </summary>
    /// <returns></returns> Average disk usage.
    public static float AverageDiskUsage()
    {
        return DBInteract.ListAllProgramData().Average(x => x.DiskUsage);
    }
    
    /// <summary>
    /// Find average network usage of database.
    /// </summary>
    /// <returns></returns> Average network usage.
    public static float AverageNetworkUsage()
    {
        return DBInteract.ListAllProgramData().Average(x => x.NetworkUsage);
    }
    
    /// <summary>
    /// Find average memory usage of database.
    /// </summary>
    /// <returns></returns> Average memory usage.
    public static float AverageMemoryUsage()
    {
        return DBInteract.ListAllProgramData().Average(x => x.MemoryUsage);
    }

    /// <summary>
    /// Find first instance of peak cpu usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak Cpu usage.
    public static ProgramData PeakCpuUsage()
    {
        var maxCpuUsage = DBInteract.ListAllProgramData().Max(x => x.CpuUsage);
        return DBInteract.ListAllProgramData().First(x => x.CpuUsage >= maxCpuUsage);
    }
    
    /// <summary>
    /// Find first instance of peak disk usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak disk usage.
    public static ProgramData PeakDisk()
    {
        var maxDiskUsage = DBInteract.ListAllProgramData().Max(x => x.CpuUsage);
        return DBInteract.ListAllProgramData().First(x => x.CpuUsage >= maxDiskUsage);
    }
    
    /// <summary>
    /// Find first instance of peak network usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak network usage.
    public static ProgramData PeakNetworkUsage()
    {
        var maxNetworkUsage = DBInteract.ListAllProgramData().Max(x => x.CpuUsage);
        return DBInteract.ListAllProgramData().First(x => x.CpuUsage >= maxNetworkUsage);
    }
    
    /// <summary>
    /// Find first instance of peak memory usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak memory usage.
    public static ProgramData PeakMemoryUsage()
    {
        var maxMemoryUsage = DBInteract.ListAllProgramData().Max(x => x.CpuUsage);
        return DBInteract.ListAllProgramData().First(x => x.CpuUsage >= maxMemoryUsage);
    }

    /// <summary>
    /// Find the total amount of network that has been consumed across all entries.
    /// </summary>
    /// <returns>Total netork usage.</returns> 
    public static float TotalNetworkUsage()
    {
        return DBInteract.ListAllProgramData().Sum(x => x.NetworkUsage);
    }
    
}