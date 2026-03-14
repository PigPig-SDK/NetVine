using System.Linq;
using Core;

namespace Infrastructure;

public static class DBArithmetic
{
    /// <summary>
    /// Find historical data of database processes. Grouped by Systems and Processes.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static List<ProgramDataHistorical> HistoricalDataProducer()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.GroupBy(x => new { x.SystemName, x.ProcessName })
                .Select(y => new ProgramDataHistorical
                {
                    SystemName = y.Key.SystemName,
                    ProcessName = y.Key.ProcessName,

                    CpuUsageAvg = y.Average(z => z.CpuUsage),
                    DiskUsageAvg = y.Average(z => z.DiskUsage),
                    NetworkUsageAvg = y.Average(z => z.NetworkUsage),
                    MemoryUsageAvg = y.Average(z => z.MemoryUsage),

                    CpuUsagePeak = y.Max(z => z.CpuUsage),
                    DiskUsagePeak = y.Max(z => z.DiskUsage),
                    NetworkUsagePeak = y.Max(z => z.NetworkUsage),
                    MemoryUsagePeak = y.Max(z => z.MemoryUsage),

                    NetworkUsageTotal = y.Sum(z => z.MemoryUsage)
                }).ToList();
        }
    }

    /// <summary>
    /// Find historical data of database processes. Grouped by Processes, and combines passed in Systems.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static List<ProgramDataHistorical> HistoricalDataProducer(List<String> systemList)
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.Where(x => systemList.Contains(x.SystemName))
                .GroupBy(x => new {x.SystemName, x.ProcessName})
                .Select(y => new ProgramDataHistorical
                {
                    SystemName = y.Key.SystemName,
                    ProcessName = y.Key.ProcessName,
            
                    CpuUsageAvg = y.Average(z => z.CpuUsage),
                    DiskUsageAvg = y.Average(z => z.DiskUsage),
                    NetworkUsageAvg = y.Average(z => z.NetworkUsage),
                    MemoryUsageAvg = y.Average(z => z.MemoryUsage),
            
                    CpuUsagePeak = y.Max(z => z.CpuUsage),
                    DiskUsagePeak = y.Max(z => z.DiskUsage),
                    NetworkUsagePeak = y.Max(z => z.NetworkUsage),
                    MemoryUsagePeak = y.Max(z => z.MemoryUsage),
            
                    NetworkUsageTotal = y.Sum(z => z.MemoryUsage)
                }).ToList();
        }
    }
}