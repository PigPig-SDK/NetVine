using Core;
using Infrastructure.Networking.Packets;

namespace Infrastructure.Networking;

public class NetworkLiveDataManager
{
    public static NetworkLiveDataManager? _instance = null;
    public event Action<ProgramData[]>? OnLiveDataRecieved;
    public static NetworkLiveDataManager Instance {
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
    public HashSet<Guid> PushHostSet = [];

    public static void SetupInstance()
    {
        if (_instance != null) throw new InvalidOperationException($"Cannot call {nameof(SetupInstance)} more than once!");
        Instance = new NetworkLiveDataManager();
    }
    public NetworkLiveDataManager()
    {
        SystemHistory.Instance.OnSnapshotTaken += OnProgramShapshot;
        NetworkManager.Instance.OnDisconnectFromHost += OnHostDisconnect;
    }

    private void OnHostDisconnect(Guid info)
    {
        if(PushHostSet.Contains(info)) PushHostSet.Remove(info);
    }

    ~NetworkLiveDataManager()
    {
        SystemHistory.Instance.OnSnapshotTaken -= OnProgramShapshot;
    }
    private void OnProgramShapshot(List<IProgramData> list)
    {
        foreach (Guid pushInfoForHost in PushHostSet)
        {
            Debug.Log("TODO: Create a way to push data to our host!");
            //var packetBytes = Packet.CreatePacket(new LiveViewDataPayload(list)).ToBytes();
            byte[] packetBytes = Packet.CreatePacket(new StringPayload("Conceptually sent latest snapshot.")).ToBytes();
            NetworkManager.Instance.SendToId(pushInfoForHost, packetBytes);
        }

    }
    public void LiveDataRecieved(ProgramData[] data)
    {
        OnLiveDataRecieved?.Invoke(data);
    }
    public void HostUpdatePushStatus(bool shouldPush)
    {
        var packetBytes = Packet.CreatePacket(new SetLiveViewPayload(shouldPush)).ToBytes();
        NetworkManager.Instance.Host?.Multicast(packetBytes);
    }
}
