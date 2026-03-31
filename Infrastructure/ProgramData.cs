using Core;
using ProtoBuf;

[ProtoContract]
public class ProgramData : IProgramData
{
    [ProtoMember(1)]
    public string SystemName { get; set; } = string.Empty;
    [ProtoMember(2)]
    public string ProcessName { get; set; } = string.Empty;
    [ProtoMember(3)]
    public float CpuUsage { get; set; }
    [ProtoMember(4)]
    public float DiskUsage { get; set; }
    [ProtoMember(5)]
    public float NetworkUsage { get; set; }
    [ProtoMember(6)]
    public float MemoryUsage { get; set; }
    [ProtoMember(7)]
    public float Timespan { get; set; }
    [ProtoMember(8)]
    public DateTime Date { get; set; }
    [ProtoMember(9)]
    public int ProcessId { get; set; }

}
