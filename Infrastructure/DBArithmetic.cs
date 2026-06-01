
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
                return CombinationQueryBuilder(db.ProgramDataTable.Where(x => systemList.Contains(x.SystemName)
                    && (!date1.HasValue || x.Date >= date1) && (!date2.HasValue || x.Date <= date2)), ComboString)
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


    static IQueryable<ProgramDataHistorical> CombinationQueryBuilder(
        IQueryable<ProgramData> inputQuery, String? combinationName = null)
    {
        return inputQuery
        .Select(x => new
        {
            SystemName = combinationName ?? x.SystemName,
            x.ProcessName,
            x.Date,
            x.CpuUsage,
            x.DiskUsage,
            x.NetworkUsage,
            x.MemoryUsage
        })
        .GroupBy(x => new { x.SystemName, x.ProcessName })
        .Select(y => new ProgramDataHistorical()
        {
            SystemName = y.Key.SystemName,
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

    public static async Task<Dictionary<string, (List<DateTime> timeStamps, List<List<IProgramData>> data)>> BarGraphHistoricalProducer(DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                return db.ProgramDataTable
                    .Where(x => (!date1.HasValue || x.Date >= date1.Value) && (!date2.HasValue || x.Date <= date2.Value))
                    .AsEnumerable()
                    .GroupBy(x => x.SystemName)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var byDate = g.GroupBy(x => x.Date)
                                          .OrderBy(x => x.Key)
                                          .ToList();
                            return (
                                timeStamps: byDate.Select(d => d.Key).ToList(),
                                data: byDate.Select(d => d.Cast<IProgramData>().ToList()).ToList()
                            );
                        }
                    );
            }
        });
    }

    /// <returns> Username, List of programs resources in tuple</returns>
    public static async Task<Dictionary<string, (List<DateTime>timeStamps, List<double> cpuUsage, List<double> ramUsage, List<double> diskUsage, List<double> netUsage)>>
        PerUserTimeline(DateTime? date1, DateTime? date2)
    {
        return await Task.Run(() =>
        {
            using (var db = new DBInteract())
            {
                var capturedData = db.ProgramDataTable
                    .Where(x => (!date1.HasValue || x.Date >= date1.Value) && (!date2.HasValue || x.Date <= date2.Value))
                    .ToList();

                return capturedData
                    .GroupBy(x => x.SystemName)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var byDate = g
                                .GroupBy(x => x.Date)
                                .OrderBy(x => x.Key)
                                .ToList();

                            return (
                                timeStamps: byDate.Select(d => d.Key).ToList(),
                                cpuUsage: byDate.Select(d => (double)d.Sum(x => x.CpuUsage)).ToList(),
                                ramUsage: byDate.Select(d => (double)d.Sum(x => x.MemoryUsage)).ToList(),
                                diskUsage: byDate.Select(d => (double)d.Sum(x => x.DiskUsage)).ToList(),
                                netUsage: byDate.Select(d => (double)d.Sum(x => x.NetworkUsage)).ToList()
                            );
                        }
                    );
            }
        });
    } 
}