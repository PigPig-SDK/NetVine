using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static UI.ViewModels.TableViewModel;

namespace UI.ViewModels
{

    /// <summary>
    /// 
    ///     Dummy View-Model for the table view. 
    ///     
    /// </summary>
    internal class TableViewModel : ViewModelBase
    {
        public TableViewModel()
        {
            PopulateTableWithDummyData();
        }
        //We can change this or replace it with something else
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

        public ObservableCollection<TableRow> TableRows { get; set; } = [];

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

        public IEnumerable<TableRow> FilteredRows => FilterRows(SearchText);

        //Change this to something better? Better format
        IEnumerable<TableRow> FilterRows(string searchIn)
        {   
            if (searchIn == "" || searchIn == null) return TableRows;
            //AI Generated LINQ for regex stuff:
            var result = Regex.Matches(searchIn, @"\(([^)]+)\)|(\w+)(?!\s*[,\w]*\))")
                .Cast<Match>()
                .Select(m =>
                {
                    var content = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                    return content.Split(',').Select(s => s.Trim()).ToList();
                })
                .ToList();

            //if a search term is alone, its returned. If its in parentheses, it must be grouped with another term.
            // (ThisApp, ThisPC) vice versa or (ThisApp) or (ThisPC) or ThisApp or ThisPC
            return result.SelectMany(row => TableRows.Where(tableRow => row.Count == 2
                                ? (row.Contains(tableRow.AppName) && row.Contains(tableRow.SystemName))
                                    || (row.Contains(tableRow.SystemName) && row.Contains(tableRow.AppName))
                                    : row.Count == 1 
                                    ? row.Contains(tableRow.SystemName) || row.Contains(tableRow.AppName) 
                                    : false));

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
