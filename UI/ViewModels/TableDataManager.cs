using Avalonia.Threading;
using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    /// <summary>
    /// Todo: make the live data show when changing from live view to historical
    /// </summary>
    public class TableDataManager
    {
        private readonly Func<bool> _isPaused;
        private readonly Func<string> _searchExpression;
        
        public Action ClearSelection { get; set; } = () => { };
        /// <summary>
        /// ObservableCollection containing the rows for the table
        /// </summary>
        public ObservableCollection<TableRow> TableRows { get; set; }

        /// <summary>
        /// Constructor, creates a new collection for the rows
        /// </summary>
        public TableDataManager(Func<bool> isPaused, Func<string> searchExpression)
        {
            _isPaused = isPaused;
            _searchExpression = searchExpression;
            TableRows = new ObservableCollection<TableRow>();
        }

        /// <summary>
        /// Contains the update logic, getting the list of up-to-date data for the live view
        /// </summary>
        /// <param name="data"></param> new data to take in
        public void UpdateLiveData(List<IProgramData> data)
        {
            //Debug.Log("Table Live updated");
            var existing = TableRows.ToDictionary(r => (r.SystemName, r.AppName));
            var incoming = data.ToDictionary(d => (d.SystemName, d.ProcessName));

            // Update or add
            foreach (var item in data)
            {
                var key = (item.SystemName, item.ProcessName);
                if (existing.TryGetValue(key, out var row))
                    row.LiveData = item;
                else
                    TableRows.Add(new TableRow { SystemName = item.SystemName, AppName = item.ProcessName, LiveData = item });
            }

            // Remove stale rows -- I guess that this is needed in order to keep things sorted. If not we can figure out a fix like dummy rows or something
            var toRemove = TableRows.Where(r => !incoming.ContainsKey((r.SystemName, r.AppName))).ToList();
           
            if (toRemove.Count > 0)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_isPaused()) return;
                    ClearSelection();

                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        var item = toRemove[i];
                        if (TableRows.Contains(item))
                        {
                            TableRows.Remove(item);
                        }
                    }

                }, DispatcherPriority.Background);
            }
        }


        /// <summary>
        /// Contains the logic to update the historical data inside of the TableRows collection
        /// </summary>
        /// <param name="data"></param>
        public void UpdateHistoricalData(List<ProgramDataHistorical> data)
        {
            Debug.Log($"UpdateHistoricalData called with {data.Count} items");

            var existing = TableRows.ToDictionary(r => (r.SystemName, r.AppName));
            var incoming = data.ToDictionary(d => (d.SystemName, d.ProcessName));

            foreach (var item in data)
            {
                var key = (item.SystemName, item.ProcessName);
                if (existing.TryGetValue(key, out var row))
                {
                    row.HistoricalData = item;
                }
                else
                {
                    TableRows.Add(new TableRow { SystemName = item.SystemName, AppName = item.ProcessName, HistoricalData = item });
                }
            }

            var toRemove = TableRows.Where(r => !incoming.ContainsKey((r.SystemName, r.AppName))).ToList();
            Debug.Log($"Removing {toRemove.Count} stale rows");
            foreach (var row in toRemove)
                TableRows.Remove(row);
        }


        /// <summary>
        /// Kills an application by its primary key (system, app)
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task KillAndRemoveByKey(string systemName, string appName)
        {
            await AppQuitter.KillProcessByNameAsync(appName);

            Dispatcher.UIThread.Post(() =>
            {
                ClearSelection();
                var row = TableRows.FirstOrDefault(r => r.SystemName == systemName && r.AppName == appName);
                if (row != null)
                    TableRows.Remove(row);
            }, DispatcherPriority.Background);
        }


    }
}