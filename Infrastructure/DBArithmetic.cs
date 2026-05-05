
namespace Infrastructure;

public static class DBArithmetic
{
    
    /// <summary>
    /// Takes in data, and calculates cumulative averages in Program Data Historical Table. If an entry doesnt exist,
    /// one is made with existing db data.
    /// </summary>
    public static void UpdatePDHTable(List<ProgramData> currentData, DBInteract db)
    {
            foreach(var data in currentData)
            {
                if(!db.ValueInPDH(data))
                {
                    var entryNotCombo =
                        TimeFrameBuilder(HistoricalQueryBuilder(db.ProgramDataTable
                        .Where(x => x.SystemName == data.SystemName && x.ProcessName == data.ProcessName))
                            .AsEnumerable()).FirstOrDefault();
                    
                    if (entryNotCombo != null)
                    {
                        db.AddPDHEntry(entryNotCombo);
                    }
                }

                if(!db.ValueInPDH(new ProgramData() { SystemName = "Combination", ProcessName = data.ProcessName }))
                {
                    var entryCombo =
                        TimeFrameBuilder(HistoricalQueryBuilder(db.ProgramDataTable
                                .Where(x => x.ProcessName == data.ProcessName), "Combination")
                            .AsEnumerable()).FirstOrDefault();
                    
                    if (entryCombo != null)
                    { 
                        db.AddPDHEntry(entryCombo);
                    }
                }
               
                var existingNotComboEntry = db.PDHTable.FirstOrDefault(x => x.SystemName == data.SystemName && x.ProcessName == data.ProcessName);
                if (existingNotComboEntry != null)
                {
                    HistoricalCumulativeUpdater(existingNotComboEntry, data);
                }
                
                var existingComboEntry = db.PDHTable.FirstOrDefault(x => x.SystemName == "Combination" && x.ProcessName == data.ProcessName);
                if (existingComboEntry != null)
                {
                    HistoricalCumulativeUpdater(existingComboEntry, data);
                }
            }
            db.SaveChanges();
    }
    
    /// <summary>
    /// Find historical data of database processes. Grouped by Systems and Processes.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static List<ProgramDataHistorical> HistoricalDataProducer(DateTime? date1, DateTime? date2)
    {
        using (var db = new DBInteract())
        {
            if (!date1.HasValue && !date2.HasValue)
            {
                return db.PDHTable.Where(x => x.SystemName != "Combination").ToList();
            }
            
            return TimeFrameBuilder(HistoricalQueryBuilder(db.ProgramDataTable.Where(x =>
                    (!date1.HasValue || x.Date >= date1)
                    && (!date2.HasValue || x.Date <= date2)))
                .AsEnumerable())
                .ToList();
        }
    }

    /// <summary>
    /// Find historical data of database processes. Grouped by Processes, and combines passed in Systems.
    /// </summary>
    /// <returns>Historical Data</returns>
    public static List<ProgramDataHistorical> HistoricalDataProducer(List<String> systemList, DateTime? date1, DateTime? date2)
    {
        using (var db = new DBInteract())
        {
            
            if (!date1.HasValue && !date2.HasValue)
            {
                return db.PDHTable.Where(x => x.SystemName == "Combination").ToList();
            }
            
            return TimeFrameBuilder(HistoricalQueryBuilder(db.ProgramDataTable.Where(x => systemList.Contains(x.SystemName) 
                && (!date1.HasValue || x.Date >= date1) && (!date2.HasValue || x.Date <= date2)),"Combination")
                .AsEnumerable())
                .ToList();
        }
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
    /// Builder method for TimeFrame, ostraciszed from QueryBuilder because of query - string sql shenanigans.
    /// </summary>
    /// <returns>Historical Data</returns>
    static IEnumerable<ProgramDataHistorical> TimeFrameBuilder(
        IEnumerable<ProgramDataHistorical> inputData)
    {
        return inputData.Select(x =>
        {
            x.TimeFrame = x.StartDate.ToString("MMMM d, yyyy h:mm tt") + " - " + x.EndDate.ToString("MMMM d, yyyy h:mm tt");
            return x;
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

    public static Dictionary<string, List<double>> LineGraphHistoricalProducer(DateTime? date1, DateTime? date2)
    {
        using (var db = new DBInteract())
        {
            
            var capturedData = db.ProgramDataTable.Where(x =>
                    (!date1.HasValue || x.Date >= date1.Value) && (!date2.HasValue || x.Date <= date2.Value)).ToList();

            var dict = new Dictionary<string, List<double>>
            {
                ["CPU"] = capturedData.Select(x => (double) x.CpuUsage).ToList(),
                ["RAM"] = capturedData.Select(x => (double) x.MemoryUsage).ToList(),
                ["DISK"] = capturedData.Select(x => (double) x.DiskUsage).ToList(),
                ["NET"] = capturedData.Select(x => (double) x.NetworkUsage).ToList()
            };
            
            return dict;
        }
    }
    
    
}