using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;


namespace UI.ViewModels
{


    //Future improvements:
    //TODO: Simplify filtering -- better syntax, dropdown menu instead?
    //TODO: Make it so that in the live feed, cosed programs aren't seen. I guess we could do this with the seen hashSet
    public partial class TableViewModel : ViewModelBase
    {
        
        public Action? OnRowsRefreshed;
        
        public TableViewModel()
        {
            _RowLookup = [];
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshotLive;
            DBInteract.OnDataAdded += OnSnapshotHistorical;
            LiveViewModel.ViewChanged += _ => NotifyColumnVisibility();
            //PopulateTableWithDummyData();

        }
        public Vector _savedOffset;

        /// <summary>
        /// Property for the text in the search bar. When this is updated, 
        /// it triggers a property change notification for the FilteredRows property, 
        /// which causes the UI to update the displayed rows based on the new search text.
        /// </summary>
        private string? _searchText;
        public string SearchText
        {
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
        //public IEnumerable<TableRow> FilteredRows => ApplySort(FilterRows(SearchText));
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

        //Main table data storage -- don't replace
        public ObservableCollection<TableRow> TableRows { get; set; } = [];


        /// <summary>
        /// Stores a lookup dictionary that maps a tuple of two string keys to their corresponding TableRow instances.
        /// </summary>
        /// <remarks>This dictionary enables efficient retrieval of TableRow objects based on a composite
        /// key consisting of two string values. Ensure that each key tuple is unique to prevent overwriting existing
        /// entries.</remarks>
        private Dictionary<(string, string), TableRow> _RowLookup;

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
        /// Gets the collection of table rows that match the current filter criteria.
        /// </summary>
        public ObservableCollection<TableRow> FilteredRows { get; } = [];


        /// <summary>
        /// Refreshes the collection of filtered rows based on the current search text and sorting criteria.
        /// </summary>
        /// <remarks>This method updates the contents of the FilteredRows collection to reflect any
        /// changes in search or sorting. Call this method after modifying the search text or sorting options to ensure
        /// the filtered view is current.</remarks>
        public void RefreshFilteredRows()
        {
            var newRows = ApplySort(FilterRows(SearchText)).ToList();
            FilteredRows.Clear();
            foreach (var row in newRows)
                FilteredRows.Add(row);
        }

        /// <summary>
        /// Processes an incoming snapshot of program data, updating the current state accordingly.
        /// </summary>
        /// <param name="data">A list of program data objects representing the latest state of the program. Cannot be null.</param>
        private void OnSnapshotLive(List<IProgramData> data)
        {
            if (!LiveViewModel.IsLive) return;

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

                RefreshFilteredRows();
            });
        }

        /// <summary>
        /// Refreshes the historical snapshot data for all table rows based on the latest database results.
        /// </summary>
        /// <remarks>This method updates each row's historical data on the UI thread, ensuring that only
        /// rows with matching database entries retain their historical information. Rows without corresponding data are
        /// cleared. After updating, the filtered rows are refreshed and the rows refreshed event is invoked. This
        /// method is intended for internal use and is not thread-safe.</remarks>
        private void OnSnapshotHistorical()
        {
            if (LiveViewModel.IsLive) return;

            var data = DBArithmetic.HistoricalDataProducer(null, null);

            var seen = new HashSet<(string, string)>();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                //broken, needs fixed (just a copy paste of the live view. This probably needs to be looked at more carefully)
                foreach (var incoming in data)
                {
                    var row = FindOrCreate(incoming.SystemName, incoming.ProcessName); // safe, on UI thread
                    row.HistoricalData = incoming;
                    seen.Add((incoming.SystemName, incoming.ProcessName));
                }

                foreach (var row in TableRows)
                {
                    if (!seen.Contains((row.SystemName, row.AppName)))
                        row.HistoricalData = null;
                }

                RefreshFilteredRows();
            });

            OnRowsRefreshed?.Invoke();
        }

    }
}
