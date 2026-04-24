using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using UI.ViewModels.SearchFilter;
using UI.Views;

namespace UI.ViewModels
{
    public class TableViewModel : ViewModelBase
    {
        // Fields
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

        private readonly TableDataManager _tableData;
        private bool _tableViewActive = false;
        private string _searchText = "";


        private bool _showCpu = true;
        private bool _showMemory = true;
        private bool _showDisk = true;
        private bool _showNetwork = true;
        private readonly ParseTree? _searchTree;
        private bool _updatePaused = false;

        private DateTime? HistoricalStart = null;
        private DateTime? HistoricalEnd = null;

        // Properties
        public TableDataManager TableData {  get { return _tableData; } }
        public string AllMenuText => (_showCpu && _showMemory && _showDisk && _showNetwork) ? "Hide All" : "Show All";
        public string CpuMenuText => _showCpu ? "Hide CPU usage" : "Show CPU usage";
        public string MemoryMenuText => _showMemory ? "Hide Memory usage" : "Show Memory usage";
        public string DiskMenuText => _showDisk ? "Hide Disk usage" : "Show Disk usage";
        public string NetworkMenuText => _showNetwork ? "Hide Network usage" : "Show Network usage";
        public bool ShowLive => LiveViewModel.IsLive;
        public bool IsAppnameVisible => true;
        public bool IsSystemNameVisible => true;
        public bool ShowHistorical => !LiveViewModel.IsLive;
        public DataGridCollectionView TableRowsView { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                Console.WriteLine("Search text updated: " + _searchText);
                
                TableFilter.Instance.UpdateSearchExpression(_searchText);

                OnPropertyChanged();
                if (LiveViewModel.IsLive) TableRowsView.Filter = string.IsNullOrWhiteSpace(value)
                    ? null
                    : FilterRow;
                OnSearchHistorical();

                TableRowsView.Refresh();
            }
        }
        

        // Context menu properties
        public bool ShowCpuLive => _showCpu && LiveViewModel.IsLive;
        public bool ShowMemoryLive => _showMemory && LiveViewModel.IsLive;
        public bool ShowDiskLive => _showDisk && LiveViewModel.IsLive;
        public bool ShowNetworkLive => _showNetwork && LiveViewModel.IsLive;

        public bool ShowCpuHistorical => _showCpu && !LiveViewModel.IsLive;
        public bool ShowMemoryHistorical => _showMemory && !LiveViewModel.IsLive;
        public bool ShowDiskHistorical => _showDisk && !LiveViewModel.IsLive;
        public bool ShowNetworkHistorical => _showNetwork && !LiveViewModel.IsLive;
        
        public IRelayCommand ToggleAllCommand => new RelayCommand(() =>
        {
            var newValue = !(_showCpu && _showMemory && _showDisk && _showNetwork);
            _showCpu = newValue;
            _showMemory = newValue;
            _showDisk = newValue;
            _showNetwork = newValue;
            OnPropertyChanged(nameof(AllMenuText));
            OnPropertyChanged(nameof(CpuMenuText));
            OnPropertyChanged(nameof(MemoryMenuText));
            OnPropertyChanged(nameof(DiskMenuText));
            OnPropertyChanged(nameof(NetworkMenuText));
            OnIsVisiblePropertiesChanged();
        });
        public IRelayCommand ToggleCpuCommand { get; }
        public IRelayCommand ToggleMemoryCommand { get; }
        public IRelayCommand ToggleDiskCommand { get; }
        public IRelayCommand ToggleNetworkCommand { get; }
        public IRelayCommand OpenTimeframeCommand { get; }



        // Constructor
        public TableViewModel()
        {
            _tableData = new TableDataManager(() => _updatePaused, () => SearchText);
            TableRowsView = new DataGridCollectionView(_tableData.TableRows);

            var lastActiveTab = ConfigManager.ReadSetting(SettingInt.LastActivePage);
            _tableViewActive = lastActiveTab == MainWindowViewModel.TableView ? true : false;

            //Event Subscriptions
            MainWindowViewModel.OnTabChanged += OnTabChanged;
            LiveViewModel.ViewChangedEvent += ViewChangedLive;
            CombinationModel.ViewChangedEvent += ViewChangedCombination;
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshotLive;
            DBInteract.OnProgramListAdded += OnSnapshotHistorical;
            MainWindowViewModel.OnSearchKeyStroke += OnSearchKeyStroke;

            //Commands
            ToggleCpuCommand = new RelayCommand(ToggleCpu);
            ToggleDiskCommand = new RelayCommand(ToggleDisk);
            ToggleMemoryCommand = new RelayCommand(ToggleMemory);
            ToggleNetworkCommand = new RelayCommand(ToggleNetwork);
            OpenTimeframeCommand = new RelayCommand(OpenTimeframe);
            PopulateTableInit();
        }

        private void OnSnapshotHistorical(List<ProgramData> programs, bool isDataLocal)
        {
            if (LiveViewModel.IsLive || !_tableViewActive || _updatePaused) return;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _tableData.UpdateHistoricalData(GetHistoricalData());
                TableRowsView.Refresh();
                ReapplySort();
                Debug.Log("OnSnapshot historical update completed");
            });
        }

        // Public Methods

        //we can uncomment these if we want to pause the update when context menu is selected.
        //This is because the context menu is automatically closed by avalonia when the table is updated.

        public void PauseUpdate()
        {
            _updatePaused = true;
        }

        public void ResumeUpdate()
        {
            _updatePaused = false;
        }

        ////////

        public void OnSearchHistorical()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _tableData.UpdateHistoricalData(GetHistoricalData());
                TableRowsView.Refresh();
                ReapplySort();
                Debug.Log("OnSnapshot historical update completed");
            });
        }


        public void OnSearchKeyStroke(string? search)
        {
            if (search == null) return;
            SearchText = search;
        }

        public void OnSnapshotLive(List<IProgramData> data)
        {
            {
                if (!LiveViewModel.IsLive || !_tableViewActive || _updatePaused) return;
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _tableData.UpdateLiveData(data);
                    TableRowsView.Refresh();
                    ReapplySort();
                });
            }
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

        // Private Methods
        private void OnIsVisiblePropertiesChanged()
        {
            OnPropertyChanged(nameof(ShowLive));
            OnPropertyChanged(nameof(ShowHistorical));

            OnPropertyChanged(nameof(ShowCpuLive));
            OnPropertyChanged(nameof(ShowCpuHistorical));
            OnPropertyChanged(nameof(ShowMemoryLive));
            OnPropertyChanged(nameof(ShowMemoryHistorical));
            OnPropertyChanged(nameof(ShowDiskLive));
            OnPropertyChanged(nameof(ShowDiskHistorical));
            OnPropertyChanged(nameof(ShowNetworkLive));
            OnPropertyChanged(nameof(ShowNetworkHistorical));
        }

        //commands for context menu
        private void ToggleCpu()
        {
            _showCpu = !_showCpu;
            OnPropertyChanged(nameof(CpuMenuText));
            OnPropertyChanged(nameof(ShowCpuLive));
            OnPropertyChanged(nameof(ShowCpuHistorical));
        }

        private void ToggleMemory()
        {
            _showMemory = !_showMemory;
            OnPropertyChanged(nameof(MemoryMenuText));
            OnPropertyChanged(nameof(ShowMemoryLive));
            OnPropertyChanged(nameof(ShowMemoryHistorical));
        }

        private void ToggleDisk()
        {
            _showDisk = !_showDisk;
            OnPropertyChanged(nameof(DiskMenuText));
            OnPropertyChanged(nameof(ShowDiskLive));
            OnPropertyChanged(nameof(ShowDiskHistorical));
        }

        private void ToggleNetwork()
        {
            _showNetwork = !_showNetwork;
            OnPropertyChanged(nameof(NetworkMenuText));
            OnPropertyChanged(nameof(ShowNetworkLive));
            OnPropertyChanged(nameof(ShowNetworkHistorical));
        }
        
        private async void OpenTimeframe()
        {
            Debug.Log("OpenTimeFrame called");
            if (LiveViewModel.IsLive || !_tableViewActive) return;
            
            var timeFrameWindow = new TimeFrameSelectionWindow();
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
            
            (DateTime? date1, DateTime? date2)? dateRange = await timeFrameWindow.ShowDialog<(DateTime?, DateTime?)?>
            (mainWindow);
            
            if (!dateRange.HasValue)
            {
                return;
            }
            
            Console.WriteLine(dateRange);
            HistoricalStart = dateRange.Value.date1;
            HistoricalEnd = dateRange.Value.date2;
            
            OnSwitchToHistorical();
            
            mainWindow.FindControl<FolderView>("FolderView").SetDateRange(HistoricalStart, HistoricalEnd);
        }

        //initial population on startup
        private void PopulateTableInit()
        {
            //replace this with code that works. Right now, does nothing
            OnSwitchToLive(); // just attempts to populate both with initial data
            OnSwitchToHistorical();
        }

        private void ViewChangedLive(bool isLive)
        {
            Debug.Log($"ViewChangedLive fired, isLive={isLive}");
            Debug.Log($"Printing Recieved Data to a file");

            if (isLive)
                OnSwitchToLive();
            else
                OnSwitchToHistorical();

        }
        
        private void ViewChangedCombination(bool isCombination)
        {
            Debug.Log($"ViewChangedCombination fired, isCombination={isCombination}");
            Debug.Log($"Printing Recieved Data to a file");

            OnSwitchToHistorical();

        }


        private void OnSwitchToLive()
        {
            Debug.Log($"OnSwitchToLive called. TableViewActive: {_tableViewActive}");
            if (!_tableViewActive) return;
            var data = SystemHistory.Instance.GetLatestPoll();

            Debug.Log($"GetLatestPoll returned {data?.Count ?? 0} items");
            Dispatcher.UIThread.Post(() =>
            {
                Debug.Log("Dispatcher post executing for live");
                _tableData.UpdateLiveData(data!);
                OnIsVisiblePropertiesChanged();
                TableRowsView.Refresh();
                Debug.Log($"live data update complete");
            });
        }

        private void OnSwitchToHistorical()
        {
            Debug.Log($"OnSwitchToHistorical called. TableViewActive: {_tableViewActive}");

            if (!_tableViewActive) return;

            Dispatcher.UIThread.Post(() =>
            {
                _tableData.UpdateHistoricalData(GetHistoricalData());
                OnIsVisiblePropertiesChanged();
                TableRowsView.Refresh();
                Debug.Log($"historical data update complete");
            });
        }
        
        private List<ProgramDataHistorical> GetHistoricalData()
        {
            return TableFilter.Instance.GetTableRowsHistorical(CombinationModel.IsCombination
                , [.. FolderViewData.SelectedUsers()]
                , HistoricalStart
                , HistoricalEnd);
        }
        

        private void OnTabChanged(int tab)
        {
             _tableViewActive = tab == MainWindowViewModel.TableView;
            if (tab == MainWindowViewModel.TableView)
            {
                if (LiveViewModel.IsLive) OnSwitchToLive();
                else OnSwitchToHistorical();
            }
        }

        private void ReapplySort()
        {
            if (TableRowsView.SortDescriptions.Count == 0) return;
            var sorts = TableRowsView.SortDescriptions.ToList();
            TableRowsView.SortDescriptions.Clear();
            foreach (var sort in sorts)
                TableRowsView.SortDescriptions.Add(sort);
        }

        private static bool TryParseSearchExpression(TableRow target)
        {
            return TableFilter.Instance.Evaluate(target);
        }

        private bool FilterRow(object obj)
        {
            if (!LiveViewModel.IsLive) return true;
            if (obj is not TableRow row) return false;
            return TryParseSearchExpression(row)
                || row.SystemName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                || row.AppName.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        }

    }
}