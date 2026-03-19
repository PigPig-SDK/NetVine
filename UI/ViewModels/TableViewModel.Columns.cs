
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI.ViewModels
{
    public enum SortColumn
    {
        None,
        PcName,
        Process,
        Cpu,
        Memory,
        Disk,
        Network,
        NetworkAvg,
        NetworkTop,
        NetworkTotal,
        CpuAvg,
        CpuTop,
        CpuTotal,
        MemoryAvg,
        MemoryTop,
        MemoryTotal,
        DiskAvg,
        DiskTop,
        DiskTotal,
    }

    public partial class TableViewModel : ViewModelBase
    {

        public enum ColumnGroup { None, Cpu, Memory, Disk, Network}

        public enum ColumnMode { Always, Live, History}
        public bool IsVisible_PcName => IsColumnVisible(SortColumn.PcName);
        public bool IsVisible_Process => IsColumnVisible(SortColumn.Process);
        public bool IsVisible_Cpu => IsColumnVisible(SortColumn.Cpu);
        public bool IsVisible_Memory => IsColumnVisible(SortColumn.Memory);
        public bool IsVisible_Disk => IsColumnVisible(SortColumn.Disk);
        public bool IsVisible_Network => IsColumnVisible(SortColumn.Network);
        public bool IsVisible_CpuAvg => IsColumnVisible(SortColumn.CpuAvg);
        public bool IsVisible_CpuTop => IsColumnVisible(SortColumn.CpuTop);
        public bool IsVisible_MemoryAvg => IsColumnVisible(SortColumn.MemoryAvg);
        public bool IsVisible_MemoryTop => IsColumnVisible(SortColumn.MemoryTop);
        public bool IsVisible_DiskAvg => IsColumnVisible(SortColumn.DiskAvg);
        public bool IsVisible_DiskTop => IsColumnVisible(SortColumn.DiskTop);
        public bool IsVisible_NetworkTotal => IsColumnVisible(SortColumn.NetworkTotal);
        public class ColumnDefinition
        {
            public string Header { get; init; }
            public SortColumn SortColumn { get; init; }
            public Func<TableRow, object> SortSelector { get; init; }
            public ColumnGroup Group { get; init; }
            public ColumnMode Mode { get; init; }
        
        }

        public static readonly Dictionary<SortColumn, ColumnDefinition> Columns = new()
        {
            [SortColumn.PcName] = new() { Header = "PC Name", Group = ColumnGroup.None, Mode = ColumnMode.Always, SortSelector = r => r.SystemName },
            [SortColumn.Process] = new() { Header = "Process", Group = ColumnGroup.None, Mode = ColumnMode.Always, SortSelector = r => r.AppName },
            [SortColumn.Cpu] = new() { Header = "CPU", Group = ColumnGroup.Cpu, Mode = ColumnMode.Live, SortSelector = r => r.LiveData?.CpuUsage ?? 0 },
            [SortColumn.Memory] = new() { Header = "Memory", Group = ColumnGroup.Memory, Mode = ColumnMode.Live, SortSelector = r => r.LiveData?.MemoryUsage ?? 0 },
            [SortColumn.Disk] = new() { Header = "Disk", Group = ColumnGroup.Disk, Mode = ColumnMode.Live, SortSelector = r => r.LiveData?.DiskUsage ?? 0 },
            [SortColumn.Network] = new() { Header = "Network", Group = ColumnGroup.Network, Mode = ColumnMode.Live, SortSelector = r => r.LiveData?.NetworkUsage ?? 0 },
            [SortColumn.CpuAvg] = new() { Header = "CPU Avg", Group = ColumnGroup.Cpu, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.CpuUsageAvg ?? 0 },
            [SortColumn.MemoryAvg] = new() { Header = "Memory Avg", Group = ColumnGroup.Memory, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.NetworkUsageAvg ?? 0 },
            [SortColumn.DiskAvg] = new() { Header = "Disk Avg", Group = ColumnGroup.Disk, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.DiskUsageAvg ?? 0 },
            [SortColumn.NetworkAvg] = new() { Header = "Network Avg", Group = ColumnGroup.Network, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.NetworkUsageAvg ?? 0 },
            [SortColumn.CpuTop] = new() { Header = "CPU Top", Group = ColumnGroup.Cpu, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.CpuUsagePeak ?? 0 },
            [SortColumn.MemoryTop] = new() { Header = "Memory Top", Group = ColumnGroup.Memory, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.MemoryUsagePeak ?? 0 },
            [SortColumn.DiskTop] = new() { Header = "Disk Top", Group = ColumnGroup.Disk, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.DiskUsagePeak ?? 0},
            [SortColumn.NetworkTop] = new() { Header = "Network Top", Group = ColumnGroup.Network, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.NetworkUsagePeak ?? 0},
            [SortColumn.NetworkTotal] = new() { Header = "Network Total", Group = ColumnGroup.Network, Mode = ColumnMode.History, SortSelector = r => r.HistoricalData?.NetworkUsageTotal ?? 0 },
        };

        private Dictionary<ColumnGroup, bool> _groupVisibility = new()
        {
            [ColumnGroup.None] = true,
            [ColumnGroup.Cpu] = true,
            [ColumnGroup.Disk] = true,
            [ColumnGroup.Memory] = true,
            [ColumnGroup.Network] = true,
        };

        public bool IsGroupVisible(ColumnGroup group) => _groupVisibility.TryGetValue(group, out bool result) && result;


        public bool IsColumnVisible(SortColumn column)
        {
            if (!Columns.TryGetValue(column, out var result))
            {
                Console.WriteLine("Error getting column info");
                return false;
            }
            return IsGroupVisible(result.Group) 
                && (result.Mode == ColumnMode.Always || 
                (result.Mode == ColumnMode.History && !LiveViewModel.IsLive)
                || (result.Mode == ColumnMode.Live && LiveViewModel.IsLive));
        }

        //sorting ?:
        private SortColumn _sortColumn;
        private bool _sortAscending;
        IEnumerable<TableRow> ApplySort(IEnumerable<TableRow> rows)
        {
            if (_sortColumn == SortColumn.None) return rows;

            return Columns.TryGetValue(_sortColumn, out var def)
                ? _sortAscending ? rows.OrderBy(def.SortSelector) : rows.OrderByDescending(def.SortSelector)
                : rows;
        }

        public void SetSort(string? header)
        {
            var newColumn = Columns.FirstOrDefault(c => c.Value.Header == header).Key;

            Console.WriteLine($"SetSort: header='{header}' mapped to={newColumn}");

            if (newColumn == _sortColumn)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = newColumn;
                _sortAscending = true;
            }

            Console.WriteLine($"Sorting by {_sortColumn} ascending={_sortAscending}");

            OnPropertyChanged(nameof(FilteredRows));
        }

        private void NotifyColumnVisibility()
        {
            foreach (var column in Enum.GetValues<SortColumn>())
                OnPropertyChanged($"IsVisible_{column}");
        }

    }
}
