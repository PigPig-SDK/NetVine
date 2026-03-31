
using ProtoBuf;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class ProgramDataPayload : IPacketPayload
{
    public PacketType PacketType => PacketType.ProgramData;
    [ProtoMember(1)]
    public bool IsForDatabase { get; set; }
    [ProtoMember(2)]
    public ProgramData[] ProgramDataArray { get; set; } = [];

    public void Execute(bool isServer, Guid id)
    {
        if (!isServer) throw new InvalidOperationException("Cannot execute on client! There is a critical error somewhere in your code!");

        foreach(var data in ProgramDataArray)
        {
            Console.WriteLine($"Program Name: {data.ProcessName} | User: {data.SystemName}");
        }
    }
}
