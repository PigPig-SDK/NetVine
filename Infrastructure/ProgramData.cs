public class ProgramData
{
    private string _systemName, _processName;
    private float _cpuUsage, _diskUsage, _networkUsage, _memoryUsage, _timespan;
    private DateTime _date;
    public string SystemName { get => _systemName; set => _systemName = value; }
    public string ProcessName { get => _processName; set => _processName = value; }
    public float CpuUsage { get => _cpuUsage; set => _cpuUsage = value; }
    public float DiskUsage { get => _diskUsage; set => _diskUsage = value; }
    public float NetworkUsage { get => _networkUsage; set => _networkUsage = value; }
    public float MemoryUsage { get => _memoryUsage; set => _memoryUsage = value; }
    public float Timespan { get => _timespan; set => _timespan = value; }
    public DateTime Date { get => _date; set => _date = value; }
    public int ProcessId { get; set; }
}
