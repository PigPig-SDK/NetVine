using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

public class PacketTests
{
    [Fact]
    public void StringPayload_Serialize_CorrectString()
    {
        byte[] bytes = Packet.CreatePacket(new StringPayload("a")).ToBytes();
        //This is arbitrary...
        Assert.Equal(15, bytes.Length);
    }
    [Fact]
    public void StringPayload_Reserialize_CorrectString()
    {
        byte[] bytes = Packet.CreatePacket(new StringPayload("Hello Testing!")).ToBytes();
        //From bytes to packet
        Packet packet = Packet.FromBytes(bytes);
        StringPayload data = packet.Deserialize<StringPayload>();

        Assert.Equal(PacketType.TextMessage, packet.PacketInfo);
        Assert.Equal("Hello Testing!", data.Value);
    }
    [Fact]
    public void StringPayload_ToObject_CorrectObject()
    {
        Packet packet = Packet.CreatePacket(new StringPayload("Hello Testing!"));

        IPacketPayload packetPayload = packet.ToObject();
        Assert.IsType<StringPayload>(packetPayload);
    }
    [Fact]
    public void ProgramData_Serialize_CorrectData()
    {
        ProgramData prog = new()
        {
            SystemName = "SystemName",
            CpuUsage = 121,
            Date = DateTime.Now,
            DiskUsage = 264,
            MemoryUsage = 44,
            NetworkUsage = 21,
            ProcessId = 400,
            ProcessName = "ProcessName",
            Timespan = 100,
        };

        ProgramDataPayload payload = new() { IsForDatabase = false, ProgramDataArray = new[] { prog } };
        byte[] bytes = Packet.CreatePacket(payload).ToBytes();
        Packet.TryFromBytes(bytes, out Packet? packet);

        Assert.NotNull(packet);

        ProgramDataPayload otherSide = packet.ToObject() as ProgramDataPayload ?? throw new InvalidOperationException("Packet did not deserialize to ProgramDataPayload");

        ProgramData othersideprogram = otherSide.ProgramDataArray[0];

        Assert.Equal(PacketType.ProgramData, otherSide.PacketType);
        Assert.Equal(prog.SystemName, othersideprogram.SystemName);
        Assert.Equal(prog.CpuUsage, othersideprogram.CpuUsage);
        Assert.Equal(prog.Date, othersideprogram.Date);
        Assert.Equal(prog.DiskUsage, othersideprogram.DiskUsage);
        Assert.Equal(prog.MemoryUsage, othersideprogram.MemoryUsage);
        Assert.Equal(prog.NetworkUsage, othersideprogram.NetworkUsage);
        Assert.NotEqual(prog.ProcessId, othersideprogram.ProcessId);
        Assert.Equal(prog.ProcessName, othersideprogram.ProcessName);
        Assert.Equal(prog.Timespan, othersideprogram.Timespan);
    }
}
