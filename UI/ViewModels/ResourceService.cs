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
        public double CpuUsage { get; private set; }
        public double RamUsage { get; private set; }
        public double DiskUsage { get; private set; }
        public double NetworkUsage { get; private set; }

        private Dictionary<string, List<double>> _cpuHistories = new();
        private Dictionary<string, List<double>> _ramHistories = new();
        private Dictionary<string, List<double>> _diskHistories = new();
        private Dictionary<string, List<double>> _networkHistories = new();
        private Dictionary<string, List<List<IProgramData>>> _liveProcessData = new();

        private Dictionary<string, (List<DateTime> timeStamps, List<double> cpuUsage, List<double> ramUsage, List<double> diskUsage, List<double> netUsage)> _historicalUsages = new();
        private Dictionary<string, (List<DateTime> timeStamps, List<List<IProgramData>> data)> _historicalProcessData = new();

        private Dictionary<string, double> _deviceTotalRam = new();

        private int MaxHistory => ConfigManager.ReadSetting(SettingInt.MaxHistory) is int m && m > 0 ? m : 60;

        public event Action? DevicesChanged;
        public event Action? DataUpdated;
        /// <summary>
        /// True : Loading, False : Not loading.
        /// </summary>
        public event Action<bool>? LoadingStatusChanged;

        private ResourceService()
        {
            ConnectedUserInfo.OnUserConnectionModified += OnUserConnectionModified;
            SystemHistory.Instance.OnSnapshotTaken += OnLocalSnapshot;
            NetworkDataManager.Instance.OnLiveDataRecieved += OnRemoteSnapshot;
            SystemHistory.Instance.OnSnapshotTakenTotal += TotalSnapshotData;
        }

        private void TotalSnapshotData(IProgramData total, int count)
        {
                CpuUsage = total.CpuUsage;
                RamUsage = total.MemoryUsage;
                DiskUsage = total.DiskUsage;
                NetworkUsage = total.NetworkUsage;
        }

        private void OnLocalSnapshot(List<IProgramData> data)
        {
            OnSnapshot(SystemHistory.Instance.SystemName, data);
            if(LiveViewModel.IsLive)
                DataUpdated?.Invoke();
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
                _liveProcessData[device] = new();
                DevicesChanged?.Invoke();
            }

            var cpu = Math.Min(data.Sum(p => p.CpuUsage), 100);
            var ram = data.Sum(p => p.MemoryUsage);
            var disk = data.Sum(p => p.DiskUsage);
            var network = data.Sum(p => p.NetworkUsage);
            AddCapped(_liveProcessData[device], data.ToList());
            AddCapped(_cpuHistories[device], cpu);
            AddCapped(_ramHistories[device], ram);
            AddCapped(_diskHistories[device], disk);
            AddCapped(_networkHistories[device], network);
        }

        public async void TimeFrameUpdate(DateTime? startDate, DateTime? endDate)
        {
            LoadingStatusChanged?.Invoke(true);
            _historicalProcessData = await DBArithmetic.BarGraphHistoricalProducer(startDate, endDate);
            _historicalUsages = await DBArithmetic.PerUserTimeline(startDate, endDate);
            LoadingStatusChanged?.Invoke(false);
            DataUpdated?.Invoke();
        }

        private void AddCapped<T>(List<T> list, T value)
        {
            list.Add(value);
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }
        /// <summary>
        /// Gets all live resource entries for the specified user and resource type;
        /// </summary>
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
        /// <summary>
        /// Gets all historical entries for the specified timestamp
        /// </summary>
        public (List<double> data, List<DateTime> timeStamps, string title) GetDatedResourceForUser(ResourceTypes selectedResource, string user)
        {
            var exists = _historicalUsages.TryGetValue(user, out var u);

            return ChartService.Instance.SelectedResource switch
            {
                ResourceTypes.CPU => (exists ? u.cpuUsage : new(), exists ? u.timeStamps : new(), $"{user} CPU History (%)"),
                ResourceTypes.RAM => (exists ? u.ramUsage : new(), exists ? u.timeStamps : new(), $"{user} RAM History (MB)"),
                ResourceTypes.Disk => (exists ? u.diskUsage : new(), exists ? u.timeStamps : new(), $"{user} Disk History (MB/s)"),
                ResourceTypes.Network => (exists ? u.netUsage : new(), exists ? u.timeStamps : new(), $"{user} Network History (MB/s)"),
                _ => (new(), new(), "Invalid")
            };
        }
        public Dictionary<string, (List<DateTime> timeStamps, List<List<IProgramData>> data)> HistoricalData => _historicalProcessData.ToDictionary();

        /// <summary>
        /// Get all live program entries for a specific
        /// </summary>
        public List<List<IProgramData>> GetProgramUsageForUser(string user)
        {
            return _liveProcessData.TryGetValue(user, out var data) ? data : new();
        }
        /// <summary>
        /// Get all live program entries for a specific
        /// </summary>
        public (List<DateTime> timeStamps, List<List<IProgramData>> data) GetDatedProgramUsageForUser(string user)
        {
            var exists = _historicalProcessData.TryGetValue(user, out var u);
            return (exists ? u.timeStamps : new(), exists ? u.data : new());
        }
    }
}