using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public partial class TableViewModel : ViewModelBase
    {
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
        public IEnumerable<TableRow> FilteredRows => ApplySort(FilterRows(SearchText));
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
    }
}
