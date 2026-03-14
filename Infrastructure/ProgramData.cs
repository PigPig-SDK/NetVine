using Core;
using ProtoBuf;

[ProtoContract]
public class ProgramData : IProgramData, DBEntry
{
    
    [ProtoMember(1)]
    private string _systemName, _processName;
    
    [ProtoMember(2)]
    private float _cpuUsage, _diskUsage, _networkUsage, _memoryUsage, _timespan;
    
    [ProtoMember(3)]
    private DateTime _date;
    
    [ProtoMember(4)]
    public string SystemName { get => _systemName; set => _systemName = value; }
    
    [ProtoMember(5)]
    public string ProcessName { get => _processName; set => _processName = value; }
    
    [ProtoMember(6)]
    public float CpuUsage { get => _cpuUsage; set => _cpuUsage = value; }
    
    [ProtoMember(7)]
    public float DiskUsage { get => _diskUsage; set => _diskUsage = value; }
    
    [ProtoMember(8)]
    public float NetworkUsage { get => _networkUsage; set => _networkUsage = value; }
    
    [ProtoMember(9)]
    public float MemoryUsage { get => _memoryUsage; set => _memoryUsage = value; }
    
    [ProtoMember(10)]
    public float Timespan { get => _timespan; set => _timespan = value; }
    
    [ProtoMember(11)]
    public DateTime Date { get => _date; set => _date = value; }
    
    [ProtoMember(12)]
    public int ProcessId { get; set; }
    
}
