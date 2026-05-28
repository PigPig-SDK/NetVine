
using Core;

namespace Infrastructure;

public static class DBArithmetic
{
    public const string ComboString = "Combination";

    /// <summary>
    /// Takes in data, and calculates cumulative averages in Program Data Historical Table. If an entry doesnt exist,
    /// one is made with existing db data.
    /// </summary>
    public static void UpdatePDHTable(List<ProgramData> currentData, DBInteract db)
    {
        var existingEntries = db.PDHTable.ToDictionary(x => (x.SystemName, x.ProcessName));

        foreach (var data in currentData)
        {
            var uniqueDBKey = (data.SystemName, data.ProcessName);

            if (!existingEntries.TryGetValue(uniqueDBKey, out var entry))
            {
                entry = HistoricalQueryBuilder(
                        db.ProgramDataTable.Where(x => x.SystemName == uniqueDBKey.Item1 && x.ProcessName == uniqueDBKey.Item2))
                    .FirstOrDefault();

                if (entry != null)
                {
                    db.PDHTable.Add(entry);
                    existingEntries[uniqueDBKey] = entry;
                }
            }

            if (entry != null)
                HistoricalCumulativeUpdater(entry, data);
        }

        db.SaveChanges();
    }
    
    /// <summary>
    /// Find historical data of database processes. Grouped by Systems and Processes.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static async Task<List<ProgramDataHistorical>> HistoricalDataProducer(DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                if (!date1.HasValue && !date2.HasValue)
                {
                    return db.PDHTable.Where(x => x.SystemName != ComboString).ToList();
                }

                return HistoricalQueryBuilder(db.ProgramDataTable.Where(x =>
                        (!date1.HasValue || x.Date >= date1)
                        && (!date2.HasValue || x.Date <= date2)))
                    .ToList();
            }
        });
    }

    /// <summary>
    /// Find historical data of database processes. Grouped by Processes, and combines passed in Systems.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static async Task<List<ProgramDataHistorical>> HistoricalDataProducer(List<String> systemList, DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                
                if (!date1.HasValue && !date2.HasValue)
                {
                    return db.PDHTable.Where(x => x.SystemName == ComboString).ToList();
                }
                
                return HistoricalQueryBuilder(db.ProgramDataTable.Where(x => systemList.Contains(x.SystemName) 
                    && (!date1.HasValue || x.Date >= date1) && (!date2.HasValue || x.Date <= date2)),ComboString)
                    .ToList();
            }
        });
    }
    
    /// <summary>
    /// Builder Helper method for Historical Data Queries so I didnt have to repeat this 10 unjillion times.
    /// </summary>
    /// <returns>Historical Data</returns>
    static IQueryable<ProgramDataHistorical> HistoricalQueryBuilder(
        IQueryable<ProgramData> inputQuery, String? combinationName = null)
    {
        return inputQuery.GroupBy(x => new { x.SystemName, x.ProcessName })
            .Select(y => new ProgramDataHistorical()
            {
                SystemName = combinationName ?? y.Key.SystemName,
                ProcessName = y.Key.ProcessName,

                StartDate = y.Min(z => z.Date).Date,
                EndDate = y.Max(z => z.Date).Date,

                ValueCount = y.Count(),

                CpuUsageAvg = y.Average(z => z.CpuUsage),
                DiskUsageAvg = y.Average(z => z.DiskUsage),
                NetworkUsageAvg = y.Average(z => z.NetworkUsage),
                MemoryUsageAvg = y.Average(z => z.MemoryUsage),

                CpuUsagePeak = y.Max(z => z.CpuUsage),
                DiskUsagePeak = y.Max(z => z.DiskUsage),
                NetworkUsagePeak = y.Max(z => z.NetworkUsage),
                MemoryUsagePeak = y.Max(z => z.MemoryUsage),

                NetworkUsageTotal = y.Sum(z => z.NetworkUsage)
            });
    }
    
    /// <summary>
    /// Builder Helper method for Historical Data Update
    /// </summary>
    /// <returns>Historical Data</returns>
    static void HistoricalCumulativeUpdater(ProgramDataHistorical existingEntry, ProgramData newEntry)
    {
        existingEntry.ValueCount++;
        
        existingEntry.CpuUsageAvg += (newEntry.CpuUsage - existingEntry.CpuUsageAvg) / existingEntry.ValueCount;
        existingEntry.DiskUsageAvg += (newEntry.DiskUsage - existingEntry.DiskUsageAvg) / existingEntry.ValueCount;
        existingEntry.NetworkUsageAvg += (newEntry.NetworkUsage - existingEntry.NetworkUsageAvg) / existingEntry.ValueCount;
        existingEntry.MemoryUsageAvg += (newEntry.MemoryUsage - existingEntry.MemoryUsageAvg) / existingEntry.ValueCount;

        existingEntry.CpuUsagePeak = MathF.Max(existingEntry.CpuUsagePeak, newEntry.CpuUsage);
        existingEntry.DiskUsagePeak = MathF.Max(existingEntry.DiskUsagePeak, newEntry.DiskUsage);
        existingEntry.NetworkUsagePeak = MathF.Max(existingEntry.NetworkUsagePeak, newEntry.NetworkUsage);
        existingEntry.MemoryUsagePeak = MathF.Max(existingEntry.MemoryUsagePeak, newEntry.MemoryUsage);
        
        existingEntry.NetworkUsageTotal +=  newEntry.NetworkUsage;
        
    }

    public static async Task<Dictionary<string, List<double>>> LineGraphHistoricalProducer(List<String> systemList,
        DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                
                var capturedData = db.ProgramDataTable.Where(x =>
                        (!date1.HasValue || x.Date >= date1.Value) && (!date2.HasValue || x.Date <= date2.Value)
                        && (systemList.Contains(x.SystemName))).ToList();

                var grouped = capturedData.GroupBy(x => x.Date).OrderBy(x => x.Key);

                var dict = new Dictionary<string, List<double>>
                {
                    ["CPU"] = grouped.Select(g => (double)g.Sum(x => x.CpuUsage)).ToList(),
                    ["RAM"] = grouped.Select(g => (double)g.Sum(x => x.MemoryUsage)).ToList(),
                    ["DISK"] = grouped.Select(g => (double)g.Sum(x => x.DiskUsage)).ToList(),
                    ["NET"] = grouped.Select(g => (double)g.Sum(x => x.NetworkUsage)).ToList()
                };

                return dict;
            }
        });    
    }
    
    public static async Task<List<List<IProgramData>>> BarGraphHistoricalProducer(List<String> systemList, DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                return db.ProgramDataTable.Where(x =>
                        (!date1.HasValue || x.Date >= date1.Value) && (!date2.HasValue || x.Date <= date2.Value) 
                        && (systemList.Contains(x.SystemName))).AsEnumerable().GroupBy(x => x.Date)
                    .Select(g => g.Cast<IProgramData>().ToList())
                    .ToList();
            }
        });
    }
    
    
}