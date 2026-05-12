using Core;
using Infrastructure;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<double> LiveCpuHistory => _cpuHistories.TryGetValue(SelectedDevice, out var h) ? h : new();
        public List<double> HistoricalCpuHistory { get; private set; } = new();

        public List<double> LiveRamHistory => _ramHistories.TryGetValue(SelectedDevice, out var h) ? h : new();
        public List<double> HistoricalRamHistory { get; private set; } = new();

        public List<double> LiveDiskHistory => _diskHistories.TryGetValue(SelectedDevice, out var h) ? h : new();
        public List<double> HistoricalDiskHistory { get; private set; } = new();

        public List<double> LiveNetworkHistory => _networkHistories.TryGetValue(SelectedDevice, out var h) ? h : new();
        public List<double> HistoricalNetworkHistory { get; private set; } = new();

        public List<List<IProgramData>> SnapshotHistory =>
   _snapshotHistories.TryGetValue(SelectedDevice, out var h) ? h : new();

        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public IEnumerable<string> KnownDevices => _cpuHistories.Keys;

        public event Action? DataUpdated;

        private int MaxHistory => ConfigManager.ReadSetting(SettingInt.MaxHistory) is int m && m > 0 ? m : 60;

        private string SelectedDevice => ChartService.Instance.SelectedDevice ?? Environment.MachineName;

        public event Action? DevicesChanged;
        public event Action<string>? DeviceDisconnected;
        private ResourceService()
        {
            SystemHistory.Instance.OnSnapshotTaken -= OnLocalSnapshot;
            SystemHistory.Instance.OnSnapshotTaken += OnLocalSnapshot;

            NetworkDataManager.Instance.OnLiveDataRecieved -= OnRemoteSnapshot;
            NetworkDataManager.Instance.OnLiveDataRecieved += OnRemoteSnapshot;
            RAMTotal = SystemHistory.Instance.GetTotalRam();

            ConnectedUserInfo.OnUserConnectionModified += OnUserConnectionModified;
        }
        private void OnLocalSnapshot(List<IProgramData> data)
        {
            OnSnapshot(Environment.MachineName, data);
        }
        private void OnRemoteSnapshot(ProgramData[] data)
        {
            if (data.Length == 0) return;
            var byDevice = data.GroupBy(p => p.SystemName);
            foreach (var group in byDevice)
                OnSnapshot(group.Key, group.Cast<IProgramData>().ToList());
        }

        private void OnUserConnectionModified(string username, bool isAdded)
        {
            if (!isAdded)
            {
                DeviceDisconnected?.Invoke(username);
            }
            DevicesChanged?.Invoke();
        }
        private void OnSnapshot(string device, List<IProgramData> data)
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

            //LiveCpuHistory.Add(cpu);
            //LiveRamHistory.Add(ram);
            //LiveDiskHistory.Add(disk);
            //LiveNetworkHistory.Add(network);

            AddCapped(_cpuHistories[device], cpu);
            AddCapped(_ramHistories[device], ram);
            AddCapped(_diskHistories[device], disk);
            AddCapped(_networkHistories[device], network);

            if (device == SelectedDevice)
            {
                CpuUsage = cpu;
                RamUsage = ram;
                DiskUsage = disk;
                NetworkUsage = network;
                DataUpdated?.Invoke();
            }
        }
        
        public void TimeFrameUpdate(DateTime? startDate, DateTime? endDate)
        {
         
            var dataDict = DBArithmetic.LineGraphHistoricalProducer(startDate, endDate);
            HistoricalCpuHistory = dataDict["CPU"];
            HistoricalRamHistory = dataDict["RAM"];
            HistoricalDiskHistory = dataDict["DISK"];
            HistoricalNetworkHistory = dataDict["NET"];
            
            DataUpdated?.Invoke();
        }


        private void AddCapped<T>(List<T> list, T value)
        {
            list.Add(value);
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }

    }
}