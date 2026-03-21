using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Infrastructure;

namespace UI.ViewModels
{
    /// <summary>
    /// Todo: make the live data show when changing from live view to historical
    /// </summary>
    public class TableDataManager
    {
        public ObservableCollection<TableRow> TableRows { get; set; }

        public TableDataManager()
        {
            TableRows = new ObservableCollection<TableRow>();
        }

        public void UpdateLiveData(List<IProgramData> data)
        {
            if (!LiveViewModel.IsLive) return;

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
            foreach (var row in toRemove)
                TableRows.Remove(row);
        }

        public void UpdateHistoricalData(List<ProgramDataHistorical> data)
        {
            if (LiveViewModel.IsLive) return;

            var existing = TableRows.ToDictionary(r => (r.SystemName, r.AppName));
            var incoming = data.ToDictionary(d => (d.SystemName, d.ProcessName));

            foreach (var item in data)
            {
                var key = (item.SystemName, item.ProcessName);
                if (existing.TryGetValue(key, out var row))
                    row.HistoricalData = item;
                else
                    TableRows.Add(new TableRow { SystemName = item.SystemName, AppName = item.ProcessName, HistoricalData = item });
            }

            var toRemove = TableRows.Where(r => !incoming.ContainsKey((r.SystemName, r.AppName))).ToList();
            foreach (var row in toRemove)
                TableRows.Remove(row);
        }
    }
}