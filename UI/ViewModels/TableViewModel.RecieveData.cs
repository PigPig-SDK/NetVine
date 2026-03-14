using Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UI.ViewModels
{
    public partial class TableViewModel : ViewModelBase
    {


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

                OnPropertyChanged(nameof(TableRows));
            });
        }



    }
}
