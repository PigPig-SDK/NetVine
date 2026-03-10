using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static UI.ViewModels.TableViewModel;

namespace UI.ViewModels
{
    //Future improvements:
    //TODO: Simplify filtering -- better syntax, dropdown menu instead?
    public class TableViewModel : ViewModelBase
    {
        public TableViewModel()
        {
            _RowLookup = [];
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshotLive;
            //PopulateTableWithDummyData();
            ToggleCpuCommand = new RelayCommand(ToggleCpu);
            ToggleDiskCommand = new RelayCommand(ToggleDisk);
            ToggleRamCommand = new RelayCommand(ToggleRam);
            ToggleNetCommand = new RelayCommand(ToggleNet);
        }

        
        
        /// <summary>
        /// Represents a row of performance metrics for a specific system application, including CPU, RAM, disk, and
        /// network usage statistics. This is also sort of a placeholder class. Planning on adding some columns.
        /// </summary>
        /// <remarks>This class is designed to hold average and maximum resource usage values, which can
        /// be useful for monitoring and analyzing application performance over time.</remarks>
        public class TableRow : ObservableObject
        {
            public string SystemName { get; set; }
            public string AppName { get; set; }

            private IProgramData? _liveData;
            public IProgramData? LiveData
            {
                get => _liveData;
                set { _liveData = value; OnPropertyChanged(); }
            }

            private float _cpuAvg;
            public float CpuAvg { 
                get => _cpuAvg; 
                set { _cpuAvg = value; OnPropertyChanged(); } 
            }

            private float _cpuMax;
            public float CpuMax { get => _cpuMax; set { _cpuMax = value; OnPropertyChanged(); } }

            private float _ramAvg;
            public float RamAvg { get => _ramAvg; set { _ramAvg = value; OnPropertyChanged(); } }

            private float _ramMax;
            public float RamMax { get => _ramMax; set { _ramMax = value; OnPropertyChanged(); } }

            private float _diskAvg;
            public float DiskAvg { get => _diskAvg; set { _diskAvg = value; OnPropertyChanged(); } }

            private float _diskMax;
            public float DiskMax { get => _diskMax; set { _diskMax = value; OnPropertyChanged(); } }

            private float _networkAvg;
            public float NetworkAvg { get => _networkAvg; set { _networkAvg = value; OnPropertyChanged(); } }

            private float _networkMax;
            public float NetworkMax { get => _networkMax; set { _networkMax = value; OnPropertyChanged(); } }

        }

        private Dictionary<(string, string), TableRow> _RowLookup;

        //Main table data storage
        public ObservableCollection<TableRow> TableRows { get; set; } = [];

        //Context Menu proprties and commands...
        public bool DisplaySystemName { get; set; } = true;
        public bool DisplayAppName { get; set; } = true;
        private bool _displayCpuAvg = true;
        public bool DisplayCpuAvg { get => _displayCpuAvg; set { _displayCpuAvg = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextCpu));}}
        private bool _displayCpuMax = true;
        public bool DisplayCpuTop { get => _displayCpuMax; set {_displayCpuMax = value; OnPropertyChanged();OnPropertyChanged(nameof(ContextMenuTextCpu)); }}
        private bool _displayRamAvg = true;
        public bool DisplayRamAvg { get => _displayRamAvg; set { _displayRamAvg = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextRam)); } }
        private bool _displayRamTop = true;
        public bool DisplayRamTop { get => _displayRamTop; set { _displayRamTop = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextRam)); } }
        private bool _displayDiskAvg = true;
        public bool DisplayDiskAvg { get => _displayDiskAvg ; set {_displayDiskAvg = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextDisk)); } }
        private bool _displayDiskTop = true;
        public bool DisplayDiskTop { get => _displayDiskTop ; set {_displayDiskTop = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextDisk)); } }
        private bool _displayNetAvg = true;
        public bool DisplayNetworkAvg { get => _displayNetAvg ; set {_displayNetAvg = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextNet)); } }
        private bool _displayNetTop = true;
        public bool DisplayNetworkTop { get => _displayNetTop; set { _displayNetTop = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContextMenuTextNet)); } }
        private string _ContextMenuSearchText(bool shown, string word) => shown? $"Hide {word}" : $"Show {word}";
        public string ContextMenuTextCpu => _ContextMenuSearchText(DisplayCpuAvg, "CPU Usage");
        public string ContextMenuTextRam => _ContextMenuSearchText(DisplayRamAvg, "Ram Usage");
        public string ContextMenuTextDisk => _ContextMenuSearchText(DisplayDiskAvg, "Disk Usage");
        public string ContextMenuTextNet => _ContextMenuSearchText(DisplayNetworkAvg, "Network Usage");
        public ICommand ToggleCpuCommand { get; set; }
        public ICommand ToggleDiskCommand { get; set; }
        public ICommand ToggleRamCommand { get; set; }
        public ICommand ToggleNetCommand { get; set; }
        private void ToggleCpu() => (DisplayCpuAvg, DisplayCpuTop) = (!DisplayCpuAvg, !DisplayCpuTop);
        private void ToggleDisk() => (DisplayDiskAvg, DisplayDiskTop) = (!DisplayDiskAvg, !DisplayDiskTop);
        private void ToggleRam() => (DisplayRamAvg, DisplayRamTop) = (!DisplayRamAvg, !DisplayRamTop);
        private void ToggleNet() => (DisplayNetworkAvg, DisplayNetworkTop) = (!DisplayNetworkAvg, !DisplayNetworkTop);

        

        /// <summary>
        /// Property for the text in the search bar. When this is updated, 
        /// it triggers a property change notification for the FilteredRows property, 
        /// which causes the UI to update the displayed rows based on the new search text.
        /// </summary>
        private string? _searchText;
        public string SearchText{
            get => _searchText ??= "";
            set
            {
                _searchText = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredRows));
            }
        }       

        //TODO: Maybe add filter class which we can test
        /// <summary>
        /// Gets the collection of table rows that match the current search criteria.
        /// Pretty redamentary and not very robust. Will be replaced with something better (like a testable class).
        /// </summary>
        /// <remarks>The returned rows are filtered based on the value of <see cref="SearchText"/>. This
        /// property is useful for retrieving a dynamic subset of rows that satisfy the search condition, such as for
        /// displaying search results in a user interface.</remarks>
        public IEnumerable<TableRow> FilteredRows => FilterRows(SearchText);
        IEnumerable<TableRow> FilterRows(string searchIn)
        {   
            if (searchIn == "" || searchIn == null) return TableRows;

            var result = Regex.Matches(searchIn, @"\(([^)]+)\)|(\w+)(?!\s*[,\w]*\))")
                .Cast<Match>()
                .Select(m =>
                {
                    var content = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                    return content.Split(',').Select(s => s.Trim()).ToList();
                })
                .ToList();

            return result.SelectMany(row => TableRows.Where(tableRow => row.Count == 2
                  ? (row.Contains(tableRow.AppName) && row.Contains(tableRow.SystemName))
                        || (row.Contains(tableRow.SystemName) && row.Contains(tableRow.AppName))
                   : 
                        row.Count == 1
                             ? row.Contains(tableRow.SystemName) || row.Contains(tableRow.AppName)
                        : 
                            false));
        }

        /// <summary>
        /// helper method to find the 
        /// </summary>
        /// <param name="sysName"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        private TableRow FindOrCreate(string sysName, string appName)
        {
            var key = (sysName, appName);
            if (_RowLookup.TryGetValue(key, out var row))
                return row;

            var newRow = new TableRow
            {
                SystemName = sysName,
                AppName = appName,
            };
            _RowLookup[key] = newRow;
            TableRows.Add(newRow);
            return newRow;
        }

        /// <summary>
        /// Processes an incoming snapshot of program data, updating the current state accordingly.
        /// </summary>
        /// <param name="data">A list of program data objects representing the latest state of the program. Cannot be null.</param>
        private void OnSnapshotLive(List<IProgramData> data)
        {
            var seen = new HashSet<(string, string)>();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                foreach (var incoming in data)
                {
                    var row = FindOrCreate(incoming.SystemName, incoming.ProcessName); // safe, on UI thread
                    row.LiveData = incoming;
                    seen.Add((incoming.SystemName, incoming.ProcessName));
                }

                foreach (var row in TableRows)
                {
                    if (!seen.Contains((row.SystemName, row.AppName)))
                        row.LiveData = null;
                }
            });
        }


        /// <summary>
        /// Populates the table with sample data for testing or demonstration purposes.
        /// </summary>
        /// <remarks>This method generates 50 rows of dummy data, randomly assigning system and
        /// application names, as well as random values for CPU, RAM, disk, and network metrics. Intended for use in
        /// development scenarios where representative data is needed to visualize or test table
        /// functionality.</remarks>
        private void PopulateTableWithDummyData()
        {
            string[] dummySysNames = { "MyComputer", "WorkPC", "GamingRig" };
            string[] dummyAppNames = { "Netvine", "Chrome", "Visual Studio", "Spotify", "Microsoft Excel" };
            var rng = new Random();
            int numberOfRows = 50;

            for (int i = 0; i < numberOfRows; i++)
            {
                TableRows.Add(new TableRow
                {
                    SystemName = dummySysNames[rng.Next(dummySysNames.Length)],
                    AppName = dummyAppNames[rng.Next(dummyAppNames.Length)],
                    CpuAvg = MathF.Round(rng.NextSingle() * 100, 2),
                    CpuMax = MathF.Round(rng.NextSingle() * 100, 2),
                    RamAvg = MathF.Round(rng.NextSingle() * 16384, 2),
                    RamMax = MathF.Round(rng.NextSingle() * 16384, 2),
                    DiskAvg = MathF.Round(rng.NextSingle() * 500, 2),
                    DiskMax = MathF.Round(rng.NextSingle() * 500, 2),
                    NetworkAvg = MathF.Round(rng.NextSingle() * 1000, 2),
                    NetworkMax = MathF.Round(rng.NextSingle() * 1000, 2),
                });
            }
        }





    }
}
