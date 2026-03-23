using Avalonia.Collections;
using Core;
using Infrastructure;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace UI.ViewModels
{
    public class TableViewModel : ViewModelBase
    {
        private TableDataManager _tableData;

        public bool ShowLive  => LiveViewModel.IsLive;

        public bool ShowHistorical => !LiveViewModel.IsLive;

        private void ViewChanged(bool b)
        {
            OnPropertyChanged(nameof(ShowLive));
            OnPropertyChanged(nameof(ShowHistorical));
            TableRowsView.Refresh();

        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                TableRowsView.Filter = string.IsNullOrWhiteSpace(value)
                    ? null
                    : FilterRow;
                TableRowsView.Refresh();
            }
        }

        private bool FilterRow(object obj)
        {
            if (obj is not TableRow row) return false;
            return row.SystemName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                || row.AppName.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        }
        public DataGridCollectionView TableRowsView { get; set; }


        /// <summary>
        /// The header names need to be converted to the actual property name for sorting
        /// </summary>
        private static readonly Dictionary<string, string> _headerToProperty = new()
        {
            ["PC Name"] = nameof(TableRow.SystemName),
            ["Process"] = nameof(TableRow.AppName),
            ["CPU"] = "LiveData.CpuUsage",
            ["Memory"] = "LiveData.MemoryUsage",
            ["CPU Avg"] = "HistoricalData.CpuUsageAvg",
            ["CPU Top"] = "HistoricalData.CpuUsagePeak",
            ["Memory Avg"] = "HistoricalData.MemoryUsageAvg",
            ["Memory Top"] = "HistoricalData.MemoryUsagePeak",
            ["Disk Avg"] = "HistoricalData.DiskUsageAvg",
            ["Disk Top"] = "HistoricalData.DiskUsagePeak",
            ["Network Avg"] = "HistoricalData.NetworkUsageAvg",
            ["Network Top"] = "HistoricalData.NetworkUsagePeak",
            ["Network Total"] = "HistoricalData.NetworkUsageTotal",
        };

        

        public TableViewModel()
        {
            _tableData = new TableDataManager();
            TableRowsView = new DataGridCollectionView(_tableData.TableRows);

            LiveViewModel.ViewChanged += ViewChanged;
             SystemHistory.Instance.OnSnapshotTaken += OnSnapshotLive;
            DBInteract.OnDataAdded += OnSnapshotHistorical;
        }

        public void OnSnapshotLive(List<IProgramData> data)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _tableData.UpdateLiveData(data);
                TableRowsView.Refresh();
            });
        }
        public void OnSnapshotHistorical()
        {
            var data = DBArithmetic.HistoricalDataProducer(null, null);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _tableData.UpdateHistoricalData(data);
                TableRowsView.Refresh();
            });
        }
        
        public void SetSort(string? header, bool isAscending)
        {
            if (header == null || !_headerToProperty.TryGetValue(header, out var path)) return;
            TableRowsView.SortDescriptions.Clear();
            TableRowsView.SortDescriptions.Add(DataGridSortDescription.FromPath(path,
                isAscending
                    ? System.ComponentModel.ListSortDirection.Ascending
                    : System.ComponentModel.ListSortDirection.Descending));
        }
    }
}

