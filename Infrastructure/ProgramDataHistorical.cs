using Core;

namespace Infrastructure;

public class ProgramDataHistorical : IProgramDataHistorical
{
    public string SystemName { get; set; }
    public string ProcessName { get; set; }
    public float CpuUsageAvg { get; set; }
    public float DiskUsageAvg { get; set; }
    public float NetworkUsageAvg { get; set; }
    public float MemoryUsageAvg { get; set; }
    public float CpuUsagePeak { get; set; }
    public float DiskUsagePeak { get; set; }
    public float NetworkUsagePeak { get; set; }
    public float MemoryUsagePeak { get; set; }
    public float NetworkUsageTotal { get; set; }
}