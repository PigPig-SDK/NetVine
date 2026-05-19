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
    /// <summary>
    /// Do not store this data!
    /// An applications ProcessID changes every time it is loaded.
    /// </summary>
    public int ProcessId { get; set; }
    /// <summary>
    /// A+B operator for simplicity
    /// NOTE: Program A is used for Date,Timespan, Systemname, so on...
    /// Everything else is a sum!
    /// </summary>
    public static ProgramData operator +(ProgramData a, ProgramData b)
    {
        return new ProgramData
        {
            SystemName = a.SystemName,
            ProcessName = a.ProcessName,
            CpuUsage = a.CpuUsage + b.CpuUsage,
            DiskUsage = a.DiskUsage + b.DiskUsage,
            NetworkUsage = a.NetworkUsage + b.NetworkUsage,
            MemoryUsage = a.MemoryUsage + b.MemoryUsage,
            Timespan = a.Timespan,
            Date = a.Date,
            ProcessId = a.ProcessId
        };
    }
}
