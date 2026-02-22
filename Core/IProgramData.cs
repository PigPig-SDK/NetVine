
namespace Core;

public interface IProgramData
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
    /// Cpu usage as a %
    /// </summary>
    public float CpuUsage { get; set; }
    /// <summary>
    /// Disk usage in MB/step
    /// </summary>
    public float DiskUsage { get; set; }
    /// <summary>
    /// Network Usage in MB/step
    /// </summary>
    public float NetworkUsage { get; set; }
    /// <summary>
    /// Memory usage in MB for an instant
    /// </summary>
    public float MemoryUsage { get; set; }
    /// <summary>
    /// The time step of the program data
    /// </summary>
    public float Timespan { get; set; }
    
    public DateTime Date { get; set; }
}
