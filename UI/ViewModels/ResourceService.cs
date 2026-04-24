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
        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

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

        private ResourceService()
        {
            SystemHistory.Instance.OnSnapshotTaken -= OnSnapshot;
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshot;
            RAMTotal = GetRAMTotal();
        }
        private double GetRAMTotal()
        {
            var status = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
            GlobalMemoryStatusEx(ref status);
            return status.ullTotalPhys / (1024.0 * 1024.0);
        }

        private void OnSnapshot(List<IProgramData> data)
        {
            LatestSnapshot = data;
            
            SnapshotHistory.Add(data.ToList());
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

            DataUpdated?.Invoke();
        }
        

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatusEx
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

    }
}