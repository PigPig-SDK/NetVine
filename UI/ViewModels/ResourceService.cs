using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Infrastructure;

namespace UI.ViewModels
{
    public class ResourceService
    {
        public static readonly ResourceService Instance = new();

        public double RAMTotal { get; private set; }

        public double CpuUsage { get; private set; }
        public double CpuUsageAvg { get; private set; }
        public double CpuUsagePeak { get; private set; }
        
        public double RamUsage { get; private set; }
        public double RamUsageAvg { get; private set; }
        public double RamUsagePeak { get; private set; }
        
        public double DiskUsage { get; private set; }
        public double DiskUsageAvg { get; private set; }
        public double DiskUsagePeak { get; private set; }
        
        public double NetworkUsage { get; private set; }
        public double NetworkUsageAvg { get; private set; }
        public double NetworkUsagePeak { get; private set; }

        public List<double> CpuHistory { get; } = new();
        public List<double> CpuAvgHistory { get; } = new();
        public List<double> CpuPeakHistory { get; } = new();
        
        public List<double> RamHistory { get; } = new();
        public List<double> RamAvgHistory { get; } = new();
        public List<double> RamPeakHistory { get; } = new();
        
        public List<double> DiskHistory { get; } = new();
        public List<double> DiskAvgHistory { get; } = new();
        public List<double> DiskPeakHistory { get; } = new();
        
        public List<double> NetworkHistory { get; } = new();
        public List<double> NetworkAvgHistory { get; } = new();
        public List<double> NetworkPeakHistory { get; } = new();

        public List<List<IProgramData>> SnapshotHistory { get; } = new();

        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public event Action? DataUpdated;

        private int MaxHistory => ConfigManager.ReadSetting(SettingInt.MaxHistory) is int m && m > 0 ? m : 60;


        private ResourceService()
        {
            SystemHistory.Instance.OnSnapshotTaken -= OnSnapshot;
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshot;
            RAMTotal = SystemHistory.Instance.GetTotalRam();
        }

        private void OnSnapshot(List<IProgramData> data)
        {
            LatestSnapshot = data;

            AddCappedSnapshot(SnapshotHistory, data.ToList());

            CpuUsage = Math.Min(data.Sum(p => p.CpuUsage), 100);
            CpuUsageAvg = data.Average(p => p.CpuUsage);
            CpuUsagePeak = Math.Max(CpuUsage, CpuUsagePeak);
            
            RamUsage = data.Sum(p => p.MemoryUsage);
            RamUsageAvg = data.Average(p => p.MemoryUsage);
            RamUsagePeak = Math.Max(RamUsage, RamUsagePeak);
            
            DiskUsage = data.Sum(p => p.DiskUsage);
            DiskUsageAvg = data.Average(p => p.DiskUsage);
            DiskUsagePeak = Math.Max(DiskUsage, DiskUsagePeak);
            
            NetworkUsage = data.Sum(p => p.NetworkUsage);
            NetworkUsageAvg = data.Average(p => p.NetworkUsage);
            NetworkUsagePeak = Math.Max(NetworkUsage, NetworkUsagePeak);
            
            CpuHistory.Add(CpuUsage);
            CpuAvgHistory.Add(CpuUsageAvg);
            CpuPeakHistory.Add(CpuUsagePeak);
            
            RamHistory.Add(RamUsage);
            RamAvgHistory.Add(RamUsageAvg);
            RamPeakHistory.Add(RamUsagePeak);
            
            DiskHistory.Add(DiskUsage);
            DiskAvgHistory.Add(DiskUsageAvg);
            DiskPeakHistory.Add(DiskUsagePeak);
            
            NetworkHistory.Add(NetworkUsage);
            NetworkAvgHistory.Add(NetworkUsageAvg);
            NetworkPeakHistory.Add(NetworkUsagePeak);

            AddCapped(CpuHistory, CpuUsage);
            AddCapped(RamHistory, RamUsage);
            AddCapped(DiskHistory, DiskUsage);
            AddCapped(NetworkHistory, NetworkUsage);

            DataUpdated?.Invoke();
        }
        


        private void AddCapped(List<double> list, double value)
        {
            list.Add(value);
            //trim to current machistoty in case it was reduced in settings
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }

        private void AddCappedSnapshot(List<List<IProgramData>> list, List<IProgramData> value)
        {
            list.Add(value);
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }

    }
}