using System;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace UI.ViewModels
{
    public class ResourceService
    {
        public static readonly ResourceService Instance = new();

        public double CpuUsage { get; private set; }
        public double RamUsage { get; private set; }
        public double DiskUsage { get; private set; }
        public double NetworkUsage { get; private set; }

        public List<double> CpuHistory { get; } = new();
        public List<double> RamHistory { get; } = new();
        public List<double> DiskHistory { get; } = new();
        public List<double> NetworkHistory { get; } = new();

        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public event Action? DataUpdated;

        private ResourceService()
        {
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshot;
        }

        private void OnSnapshot(List<IProgramData> data)
        {
            LatestSnapshot = data;
            CpuUsage = Math.Min(data.Sum(p => p.CpuUsage), 100);
            RamUsage = data.Sum(p => p.MemoryUsage);
            DiskUsage = data.Sum(p => p.DiskUsage);
            NetworkUsage = data.Sum(p => p.NetworkUsage);

            CpuHistory.Add(CpuUsage);
            RamHistory.Add(RamUsage);
            DiskHistory.Add(DiskUsage);
            NetworkHistory.Add(NetworkUsage);

            DataUpdated?.Invoke();
        }
    }
}