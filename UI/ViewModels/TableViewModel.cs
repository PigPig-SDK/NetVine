using CommunityToolkit.Mvvm.Input;
using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static UI.ViewModels.TableViewModel;

namespace UI.ViewModels
{
    //Future improvements:
    //TODO: UpdateData -- listerner for a data update from the infrastructure.
    //TODO: Simplify filtering -- better syntax, dropdown menu instead?
    //TODO: Add more columns, like current usage. Be sure these are toggleable in the context menu.

    internal class TableViewModel : ViewModelBase
    {
        public TableViewModel()
        {
            PopulateTableWithDummyData();
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
        public class TableRow
        {
            public string SystemName { get; set; }
            public string AppName { get; set; }
            public float CpuAvg { get; set; }
            public float CpuMax { get; set; }
            public float RamAvg { get; set; }
            public float RamMax { get; set; }
            public float DiskAvg { get; set; }
            public float DiskMax { get; set; }
            public float NetworkAvg { get; set; }
            public float NetworkMax { get; set; }
        }

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
            //return Regex.Matches(searchIn, @"\(([^)]+)\)|(\w+)(?!\s*[,\w]*\))").Cast<Match>().Select(m => { var content = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value; return content.Split(',').Select(s => s.Trim()).ToList(); }).ToList().SelectMany(row => TableRows.Where(tableRow => row.Count == 2 ? (row.Contains(tableRow.AppName) && row.Contains(tableRow.SystemName)) || (row.Contains(tableRow.SystemName) && row.Contains(tableRow.AppName)) : row.Count == 1 ? row.Contains(tableRow.SystemName) || row.Contains(tableRow.AppName) : false));
        }

        private void OnSnapshot(List<IProgramData> data)
        {
            //when data comes in
        }

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
