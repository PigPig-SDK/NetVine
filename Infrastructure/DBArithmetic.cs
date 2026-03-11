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
    /// Find average cpu usage between 2 dates.
    /// </summary>
    /// <param name="date1"></param>
    /// <param name="date2"></param>
    /// <returns></returns>
    public static float AverageCpuUsage(DateTime date1, DateTime date2)
    {
        return DBInteract.ListBetweenDates(date1, date2).Average(x => x.CpuUsage);
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
    /// Find average disk usage between two dates.
    /// </summary>
    /// <returns></returns> Average disk usage.
    public static float AverageDiskUsage(DateTime date1, DateTime date2)
    {
        return DBInteract.ListBetweenDates(date1, date2).Average(x => x.DiskUsage);
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
    /// Find average network usage between two dates.
    /// </summary>
    /// <returns></returns> Average network usage.
    public static float AverageNetworkUsage(DateTime date1, DateTime date2)
    {
        return DBInteract.ListBetweenDates(date1, date2).Average(x => x.NetworkUsage);
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
    /// Find average memory usage between two dates.
    /// </summary>
    /// <returns></returns> Average memory usage.
    public static float AverageMemoryUsage(DateTime date1, DateTime date2)
    {
        return DBInteract.ListBetweenDates(date1, date2).Average(x => x.MemoryUsage);
    }
    
    /// <summary>
    /// Find first instance of peak cpu usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak Cpu usage.
    public static ProgramData PeakCpuUsage()
    {
        var maxCpuUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListAll().First(x => x.CpuUsage >= maxCpuUsage);
    }
    
    /// <summary>
    /// Find first instance of peak cpu between two dates. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak Cpu usage.
    public static ProgramData PeakCpuUsage(DateTime date1, DateTime date2)
    {
        var maxCpuUsage = DBInteract.ListAll().Max(x => x.CpuUsage);
        return DBInteract.ListBetweenDates(date1, date2).First(x => x.CpuUsage >= maxCpuUsage);
    }
    
    /// <summary>
    /// Find first instance of peak disk usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak disk usage.
    public static ProgramData PeakDiskUsage()
    {
        var maxDiskUsage = DBInteract.ListAll().Max(x => x.DiskUsage);
        return DBInteract.ListAll().First(x => x.DiskUsage >= maxDiskUsage);
    }
    
    /// <summary>
    /// Find first instance of peak disk between two dates. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak disk usage.
    public static ProgramData PeakDiskUsage(DateTime date1, DateTime date2)
    {
        var maxDiskUsage = DBInteract.ListAll().Max(x => x.DiskUsage);
        return DBInteract.ListBetweenDates(date1, date2).First(x => x.DiskUsage >= maxDiskUsage);
    }
    
    /// <summary>
    /// Find first instance of peak network usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak network usage.
    public static ProgramData PeakNetworkUsage()
    {
        var maxNetworkUsage = DBInteract.ListAll().Max(x => x.NetworkUsage);
        return DBInteract.ListAll().First(x => x.NetworkUsage >= maxNetworkUsage);
    }
    
    /// <summary>
    /// Find first instance of peak network between two dates. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak network usage.
    public static ProgramData PeakNetworkUsage(DateTime date1, DateTime date2)
    {
        var maxNetworkUsage = DBInteract.ListAll().Max(x => x.NetworkUsage);
        return DBInteract.ListBetweenDates(date1, date2).First(x => x.NetworkUsage >= maxNetworkUsage);
    }
    
    /// <summary>
    /// Find first instance of peak memory usage of database. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak memory usage.
    public static ProgramData PeakMemoryUsage()
    {
        var maxMemoryUsage = DBInteract.ListAll().Max(x => x.MemoryUsage);
        return DBInteract.ListAll().First(x => x.MemoryUsage >= maxMemoryUsage);
    }
    
    /// <summary>
    /// Find first instance of peak memory between two dates. Returns ProgramData in order to retrieve date of peak.
    /// </summary>
    /// <returns></returns> ProgramData of peak memory usage.
    public static ProgramData PeakMemoryUsage(DateTime date1, DateTime date2)
    {
        var maxMemoryUsage = DBInteract.ListAll().Max(x => x.MemoryUsage);
        return DBInteract.ListBetweenDates(date1, date2).First(x => x.CpuUsage >= maxMemoryUsage);
    }

    /// <summary>
    /// Find the total amount of network that has been consumed across all entries.
    /// </summary>
    /// <returns>Total network usage.</returns> 
    public static float TotalNetworkUsage()
    {
        return DBInteract.ListAll().Sum(x => x.NetworkUsage);
    }
    
    /// <summary>
    /// Find the total amount of network that has been consumed across entries between two dates.
    /// </summary>
    /// <returns>Total network usage.</returns> 
    public static float TotalNetworkUsage(DateTime date1, DateTime date2)
    {
        return DBInteract.ListBetweenDates(date1, date2).Sum(x => x.NetworkUsage);
    }
    
}