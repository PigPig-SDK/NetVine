using Core;
using Infrastructure.Networking.Packets;
using System.Collections.Generic;

namespace Infrastructure.Networking;

public class NetworkDataManager
{
    public static NetworkDataManager? _instance = null;
    public event Action<ProgramData[]>? OnLiveDataRecieved;
    public HashSet<Guid> LivePushHostSet = [];
    public bool IsHostExpectingLiveData = false;
    private readonly Dictionary<string, double> _remoteUsedRam = new();
    private readonly Dictionary<string, double> _remoteTotalRam = new();
    public static NetworkDataManager Instance {
        get
        {
            if(_instance is null) throw new InvalidOperationException($"Call {nameof(SetupInstance)} before accessing instance!!");
            return _instance;
        } 
        private set 
        { 
            _instance = value; 
        } 
    }
    public void HostSendLivePayload(bool isLive)
    {
        Host? host = NetworkManager.Instance.Host;
        if (host is null) return;
        IsHostExpectingLiveData = isLive;//Yes we expect it.
        SetLiveViewPayload setLiveViewPayload = new(isLive);
        Packet packet = Packet.CreatePacket(setLiveViewPayload);
        host?.Multicast(packet.ToBytes());
    }
    public static void SetupInstance()
    {
        if (_instance != null) throw new InvalidOperationException($"Cannot call {nameof(SetupInstance)} more than once!");
        Instance = new NetworkDataManager();
    }
    public NetworkDataManager()
    {
        SystemHistory.Instance.OnSnapshotTaken += OnProgramShapshot;
        DBInteract.OnProgramListAdded += OnDBSnapshot;
        NetworkManager.Instance.OnDisconnectFromHost += OnHostDisconnect;
    }
    ~NetworkDataManager()
    {
        SystemHistory.Instance.OnSnapshotTaken -= OnProgramShapshot;
    }
    private void OnDBSnapshot(List<ProgramData> programs, bool isDataLocal)
    {
        if(!isDataLocal) return;//Only submit our unique data.

        ProgramDataPayload programdata = new()
        {
            IsForDatabase = false,
            ProgramDataArray = programs.ToArray(),

            UsedRamMb = SystemHistory.Instance.GetUsedRam(),
            TotalRamMb = SystemHistory.Instance.GetTotalRam()
        };
        byte[] packetBytes = Packet.CreatePacket(programdata).ToBytes();

        NetworkManager.Instance.SendToAllHosts(packetBytes);
    }
    private void OnProgramShapshot(List<IProgramData> list)
    {
        //When we take a snapshot, push the data to the host.
        foreach (Guid pushInfoForHost in LivePushHostSet)
        {
            ProgramDataPayload programdata = new() { IsForDatabase = false, ProgramDataArray = list.Cast<ProgramData>().ToArray() };
            byte[] packetBytes = Packet.CreatePacket(programdata).ToBytes();
            NetworkManager.Instance.SendToId(pushInfoForHost, packetBytes);
        }
    }
    private void OnHostDisconnect(Guid info, ConnectionInfo connectionInfo)
    {
        LivePushHostSet.Remove(info);
    }
    public void LiveDataRecieved(ProgramData[] data)
    {
        //Remind user we are not in the mood.
        if (IsHostExpectingLiveData == false)
        {
            HostUpdatePushStatus(false);
            return;
        }
        OnLiveDataRecieved?.Invoke(data);
    }
    public void HostUpdatePushStatus(bool shouldPush)
    {
        IsHostExpectingLiveData = shouldPush;
        var packetBytes = Packet.CreatePacket(new SetLiveViewPayload(shouldPush)).ToBytes();
        NetworkManager.Instance.Host?.Multicast(packetBytes);
    }

    public double GetRemoteUsedRam(string device)
    {
        return _remoteUsedRam.TryGetValue(device, out var value)
            ? value
            : 0;
    }

    public double GetRemoteTotalRam(string device)
    {
        return _remoteTotalRam.TryGetValue(device, out var value)
            ? value
            : 0;
    }

    public void StoreRemoteMetrics(
    string device,
    double usedRam,
    double totalRam)
    {
        if (string.IsNullOrWhiteSpace(device))
            return;

        _remoteUsedRam[device] = usedRam;
        _remoteTotalRam[device] = totalRam;
    }
}
