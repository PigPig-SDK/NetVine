using ProtoBuf;
using System.Diagnostics;

namespace Infrastructure.Networking.Packets;

[ProtoContract]
public class CloseProcessPayload : IPacketPayload
{
    [ProtoMember(1)]
    public PacketType PacketType => PacketType.CloseProcessPacket;
    [ProtoMember(2)]
    public string ProcessName { get; set; } = string.Empty;

    public CloseProcessPayload() { }

    public CloseProcessPayload(string processName) {
        ProcessName = processName;
    }

    public void Execute(bool isServer, Guid id)
    {
        if (isServer) return;//Do not run on server!
        if (!ConfigManager.ReadSettingBool(SettingInt.AllowHostManipulation)) return;

        Process.GetProcessesByName(ProcessName).ToList().ForEach(p => p.Kill());
    }
}
