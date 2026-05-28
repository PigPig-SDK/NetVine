using Core;
using Infrastructure;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace UI.ViewModels
{
    public class ResourceService
    {
        public static readonly ResourceService Instance = new();

        public double RAMTotal { get; private set; }
        public double CpuUsage { get; private set; }
        public double RamUsage { get; private set; }
        public double DiskUsage { get; private set; }
        public double NetworkUsage { get; private set; }

        private Dictionary<string, List<double>> _cpuHistories = new();
        private Dictionary<string, List<double>> _ramHistories = new();
        private Dictionary<string, List<double>> _diskHistories = new();
        private Dictionary<string, List<double>> _networkHistories = new();
        private Dictionary<string, List<List<IProgramData>>> _snapshotHistories = new();
        private Dictionary<string, (List<DateTime> timeStamps, List<double> cpuUsage, List<double> ramUsage, List<double> diskUsage, List<double> netUsage)> _networkUsages = new();
        public string SelectedDevice => ChartService.Instance.SelectedDevice ?? SystemHistory.Instance.SystemName;
        public List<List<IProgramData>> SnapshotHistory => _snapshotHistories.TryGetValue(SelectedDevice, out var h) ? h : new();
        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public event Action? DataUpdated;

        private int MaxHistory => ConfigManager.ReadSetting(SettingInt.MaxHistory) is int m && m > 0 ? m : 60;

        public event Action? DevicesChanged;
        public event Action<string>? DeviceDisconnected;
        private ResourceService()
        {
            ConnectedUserInfo.OnUserConnectionModified += OnUserConnectionModified;
            SystemHistory.Instance.OnSnapshotTaken += OnLocalSnapshot;
            NetworkDataManager.Instance.OnLiveDataRecieved += OnRemoteSnapshot;
            RAMTotal = SystemHistory.Instance.GetTotalRam();
        }
        private void OnLocalSnapshot(List<IProgramData> data)
        {
            OnSnapshot(SystemHistory.Instance.SystemName, data);
        }
        private void OnRemoteSnapshot(ProgramData[] data)
        {
            if (data.Length == 0) return;

            var byDevice = data.GroupBy(p => p.SystemName);
            foreach (var group in byDevice)
            {
                if (group.Key.Equals(SystemHistory.Instance.SystemName, StringComparison.OrdinalIgnoreCase)) continue;
                OnSnapshot(group.Key, group.Cast<IProgramData>().ToList());
            }
        }

        private void OnUserConnectionModified(string username, bool isAdded)
        {
            if (!isAdded)
            {
                DeviceDisconnected?.Invoke(username);
            }
            DevicesChanged?.Invoke();
        }
        internal void OnSnapshot(string device, List<IProgramData> data)
        {
            if (!_cpuHistories.ContainsKey(device))
            {
                _cpuHistories[device] = new();
                _ramHistories[device] = new();
                _diskHistories[device] = new();
                _networkHistories[device] = new();
                _snapshotHistories[device] = new();

                DevicesChanged?.Invoke();
            }

            AddCapped(_snapshotHistories[device], data.ToList());

            var cpu = Math.Min(data.Sum(p => p.CpuUsage), 100);
            
            var ram = data.Sum(p => p.MemoryUsage);
            var disk = data.Sum(p => p.DiskUsage);
            var network = data.Sum(p => p.NetworkUsage);

            DataUpdated?.Invoke();
            AddCapped(_cpuHistories[device], cpu);
            AddCapped(_ramHistories[device], ram);
            AddCapped(_diskHistories[device], disk);
            AddCapped(_networkHistories[device], network);
        }
        
        public async void TimeFrameUpdate(DateTime? startDate, DateTime? endDate)
        {
            _networkUsages = await DBArithmetic.PerUserTimeline(startDate, endDate);
            DataUpdated?.Invoke();
        }


        private void AddCapped<T>(List<T> list, T value)
        {
            list.Add(value);
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }

        public (List<double> data, string title) GetResourceForUser(ResourceTypes selectedResource, string user)
        {
            return ChartService.Instance.SelectedResource switch
            {
                ResourceTypes.CPU => (data: _cpuHistories.TryGetValue(user, out var data) ? data : new(), title: $"{user} CPU (%)"),
                ResourceTypes.RAM => (data: _ramHistories.TryGetValue(user, out var data) ? data : new(), title: $"{user} RAM (MB)"),
                ResourceTypes.Disk => (data: _diskHistories.TryGetValue(user, out var data) ? data : new(), title: $"{user} Disk (MB/s)"),
                ResourceTypes.Network => (data: _networkHistories.TryGetValue(user, out var data) ? data : new(), title: $"{user} Network (MB/s)"),
                _ => (data: new(), title: "Invalid")
            };
        }

        public (List<double> data, List<DateTime> timeStamps, string title) GetDatedResourceForUser(ResourceTypes selectedResource, string user)
        {
            var exists = _networkUsages.TryGetValue(user, out var u);

            return ChartService.Instance.SelectedResource switch
            {
                ResourceTypes.CPU => (exists ? u.cpuUsage : new(), exists ? u.timeStamps : new(), $"{user} CPU History (%)"),
                ResourceTypes.RAM => (exists ? u.ramUsage : new(), exists ? u.timeStamps : new(), $"{user} RAM History (MB)"),
                ResourceTypes.Disk => (exists ? u.diskUsage : new(), exists ? u.timeStamps : new(), $"{user} Disk History (MB/s)"),
                ResourceTypes.Network => (exists ? u.netUsage : new(), exists ? u.timeStamps : new(), $"{user} Network History (MB/s)"),
                _ => (new(), new(), "Invalid")
            };
        }
    }
}