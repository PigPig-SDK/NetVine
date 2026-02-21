namespace Infrastructure;

public struct IoCounters
{
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;

    public static IoCounters operator +(IoCounters a, IoCounters b) => new()
    {
        ReadOperationCount = a.ReadOperationCount + b.ReadOperationCount,
        WriteOperationCount = a.WriteOperationCount + b.WriteOperationCount,
        OtherOperationCount = a.OtherOperationCount + b.OtherOperationCount,
        ReadTransferCount = a.ReadTransferCount + b.ReadTransferCount,
        WriteTransferCount = a.WriteTransferCount + b.WriteTransferCount,
        OtherTransferCount = a.OtherTransferCount + b.OtherTransferCount,
    };
}
