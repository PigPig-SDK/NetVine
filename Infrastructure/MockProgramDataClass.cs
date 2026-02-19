using Core;
namespace Infrastructure;

/// <summary>
/// TODO: REMOVE ME!
/// </summary>
public class MockProgramDataClass : IProgramData
{
    private string _systemName, _processName;
    private float _cpuUsage, _diskUsage, _networkUsage, _memoryUsage, _timespan;
    public string SystemName { get => _systemName; set => _systemName = value; }
    public string ProcessName { get => _processName; set => _processName = value; }
    public float CpuUsage { get => _cpuUsage; set => _cpuUsage = value; }
    public float DiskUsage { get => _diskUsage; set => _diskUsage = value; }
    public float NetworkUsage { get => _networkUsage; set => _networkUsage = value; }
    public float MemoryUsage { get => _memoryUsage; set => _memoryUsage = value; }
    public float Timespan { get => _timespan; set => _timespan = value; }
    public int ProcessId { get; set; }
}
