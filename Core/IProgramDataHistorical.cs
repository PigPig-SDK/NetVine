namespace Core;

public interface IProgramDataHistorical
{
    /// <summary>
    /// The SystemName of the data
    /// </summary>
    public string SystemName { get; set; }
    /// <summary>
    /// The name of the process
    /// </summary>
    public string ProcessName { get; set; }
    /// <summary>
    /// The name of the process
    /// </summary>
    public string TimeFrame { get; set; }
    
    
    /// <summary>
    /// Cpu usage as a avg %
    /// </summary>
    public float CpuUsageAvg { get; set; }
    /// <summary>
    /// Disk usage in avg MB/step
    /// </summary>
    public float DiskUsageAvg { get; set; }
    /// <summary>
    /// Network Usage in avg MB/step
    /// </summary>
    public float NetworkUsageAvg { get; set; }
    /// <summary>
    /// Memory usage in avg MB
    /// </summary>
    public float MemoryUsageAvg { get; set; }
    
    
    /// <summary>
    /// Cpu usage as a peak %
    /// </summary>
    public float CpuUsagePeak { get; set; }
    /// <summary>
    /// Disk usage in peak MB/step
    /// </summary>
    public float DiskUsagePeak { get; set; }
    /// <summary>
    /// Network Usage in peak MB/step
    /// </summary>
    public float NetworkUsagePeak { get; set; }
    /// <summary>
    /// Memory usage in MB at peak
    /// </summary>
    public float MemoryUsagePeak { get; set; }
    
    
    /// <summary>
    /// Total Network Usage
    /// </summary>
    public float NetworkUsageTotal { get; set; }
}