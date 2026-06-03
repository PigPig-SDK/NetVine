using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure
{


    using NameDateMap = ConcurrentDictionary<string, DateTime>;

    public class DbCatchup
    {
        private static DbCatchup? _instance = null;
        public static DbCatchup  Instance
        {
            get
            {
                if (_instance == null) throw new InvalidOperationException("DbCatchup instance is not setup!");
                return _instance;
            }
            private set
            {
                if (_instance != null) throw new InvalidOperationException("Only one DbCatchup instance is allowed!");
                _instance = value;
            }
        }

        private ConcurrentDictionary<string, DateTime> _dateTimeLastDisonnected;
        private string FileName => MockDataProducer.IsBeingUsed()? "mockcatchupfile.json" : "catchupfile.json";

        private DbCatchup()
        {
            LoadLastDisonnectedFromJsonFile();
            Core.Debug.Log("Loading dbcatchup");
            _dateTimeLastDisonnected ??= new();
            SaveLastDisonnectedToJsonFile();
            ConnectedUserInfo.OnUserConnectionModified += CatchUp;
        }

        public static void OnShutdown()
        {
            Instance.HostDisconnects();
        }

        public static void SetupInstance()
        {
            _instance = new DbCatchup();
        }

        public void CatchUp(string clientName, bool isAdded)
        {
            if (isAdded)
            {
                var lastConnectionHad = LastConnected(clientName);
                Core.Debug.Log($"Sending catchup data request: {lastConnectionHad} through {DateTime.Now.AddSeconds(1)}");

                var payload = new DateRequestPayload(lastConnectionHad, DateTime.Now.AddSeconds(1));
                NetworkManager.Instance.SendToUser(clientName, Packet.CreatePacket(payload).ToBytes());

                _dateTimeLastDisonnected[clientName] = DateTime.Now;
                SaveLastDisonnectedToJsonFile();
            }
            else ClientDisconnects(clientName);
        }

        public DateTime LastConnected(string clientName)
        {
            if (!_dateTimeLastDisonnected.TryGetValue(clientName, out DateTime lastConnected))
            {
                lastConnected = DateTime.MinValue;
                Core.Debug.Log($"fetch from last connected: {clientName} not in Map. making min date: {lastConnected}");
            }
            return lastConnected;
        }

        public void ClientDisconnects(string clientName)
        {
            Core.Debug.Log($"{clientName} has disconnected. Saving date to map at {DateTime.Now}");
            _dateTimeLastDisonnected[clientName] = DateTime.Now;
            SaveLastDisonnectedToJsonFile();
        }

        //call this on exit too
        public void HostDisconnects()
        {
            Core.Debug.Log("Host has disconnected! Attempting to save times ...");
            foreach (string client in _dateTimeLastDisonnected.Keys)
            {
                bool clientIsConnected = ConnectedUserInfo.IsUserConnected(client);
                if (clientIsConnected)
                    _dateTimeLastDisonnected[client] = DateTime.Now;
            }
            SaveLastDisonnectedToJsonFile();
        }

        private void LoadLastDisonnectedFromJsonFile()
        {
            if (!JsonLoader.TryLoadFromFile<Dictionary<string, DateTime>>(out var cd, FileName))
            {
                Core.Debug.Log("Json load Failed! Creating new save file: CatchupFile.json!");
                _dateTimeLastDisonnected = new();
                SaveLastDisonnectedToJsonFile();
            }
            else _dateTimeLastDisonnected = new NameDateMap(cd!);
        }

        private void SaveLastDisonnectedToJsonFile()
        {
            //JsonLoader.SaveToFile<NameDateMap>(_dateTimeLastDisonnected, _fileName);
            if (!JsonLoader.TrySaveToFile<NameDateMap>(_dateTimeLastDisonnected, FileName))
                Core.Debug.Log("Json save Failed!");
        }
    }
}
