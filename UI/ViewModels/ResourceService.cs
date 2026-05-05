using Core;
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
        public double RamUsage { get; private set; }
        public double DiskUsage { get; private set; }
        public double NetworkUsage { get; private set; }
        
        public List<double> LiveCpuHistory { get; } = new();
        public List<double> HistoricalCpuHistory { get; private set; } = new();
        
        public List<double> LiveRamHistory { get; } = new();
        public List<double> HistoricalRamHistory { get; private set; } = new();
        
        public List<double> LiveDiskHistory { get; } = new();
        public List<double> HistoricalDiskHistory { get; private set; } = new();
        
        public List<double> LiveNetworkHistory { get; } = new();
        public List<double> HistoricalNetworkHistory { get; private set; } = new();

        public List<List<IProgramData>> SnapshotHistory { get; } = new();

        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public event Action? DataUpdated;

        private ResourceService()
        {
            SystemHistory.Instance.OnSnapshotTaken -= OnSnapshot;
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshot;
            RAMTotal = SystemHistory.Instance.GetTotalRam();
        }

        private void OnSnapshot(List<IProgramData> data)
        {
            LatestSnapshot = data;
            
            SnapshotHistory.Add(data.ToList());
            
            CpuUsage = Math.Min(data.Sum(p => p.CpuUsage), 100);
            RamUsage = data.Sum(p => p.MemoryUsage);
            DiskUsage = data.Sum(p => p.DiskUsage);
            NetworkUsage = data.Sum(p => p.NetworkUsage);
            
            LiveCpuHistory.Add(CpuUsage);
            LiveRamHistory.Add(RamUsage);
            LiveDiskHistory.Add(DiskUsage);
            LiveNetworkHistory.Add(NetworkUsage);

            DataUpdated?.Invoke();
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



    }
}