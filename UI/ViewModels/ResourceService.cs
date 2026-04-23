using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace UI.ViewModels
{
    public class ResourceService
    {
        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

        public static readonly ResourceService Instance = new();

        public double RAMTotal { get; private set; }

        public double CpuUsage { get; private set; }
        public double RamUsage { get; private set; }
        public double DiskUsage { get; private set; }
        public double NetworkUsage { get; private set; }

        public List<double> CpuHistory { get; } = new();
        public List<double> RamHistory { get; } = new();
        public List<double> DiskHistory { get; } = new();
        public List<double> NetworkHistory { get; } = new();

        public List<List<IProgramData>> SnapshotHistory { get; } = new();

        public List<IProgramData> LatestSnapshot { get; private set; } = new();

        public event Action? DataUpdated;

        private int MaxHistory => ConfigManager.ReadSetting(SettingInt.MaxHistory) is int m && m > 0 ? m : 60;


        private ResourceService()
        {
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

            AddCappedSnapshot(SnapshotHistory, data.ToList());

            CpuUsage = Math.Min(data.Sum(p => p.CpuUsage), 100);
            RamUsage = data.Sum(p => p.MemoryUsage);
            DiskUsage = data.Sum(p => p.DiskUsage);
            NetworkUsage = data.Sum(p => p.NetworkUsage);

            CpuHistory.Add(CpuUsage);
            RamHistory.Add(RamUsage);
            DiskHistory.Add(DiskUsage);
            NetworkHistory.Add(NetworkUsage);

            AddCapped(CpuHistory, CpuUsage);
            AddCapped(RamHistory, RamUsage);
            AddCapped(DiskHistory, DiskUsage);
            AddCapped(NetworkHistory, NetworkUsage);

            DataUpdated?.Invoke();
        }

        private void AddCapped(List<double> list, double value)
        {
            list.Add(value);
            // trim to current MaxHistory in case it was reduced in settings
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
        }

        private void AddCappedSnapshot(List<List<IProgramData>> list, List<IProgramData> value)
        {
            list.Add(value);
            while (list.Count > MaxHistory)
                list.RemoveAt(0);
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