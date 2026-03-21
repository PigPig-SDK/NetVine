using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.Linq;
using UI.ViewModels;

namespace UI;

public partial class TableView : UserControl
{
    private Dictionary<string, bool> _sortDirections = new();

    public TableView()
    {
        InitializeComponent();
        DataContext = new TableViewModel();
    }

    private void DataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        foreach (var column in MyDataGrid.Columns)
            _sortDirections[column.Header?.ToString() ?? ""] = true;
    }

    private void DataGrid_Sorting(object? sender, DataGridColumnEventArgs e)
    {
        if (DataContext is TableViewModel vm)
        {
            string header = e.Column.Header?.ToString() ?? "";
            if (!_sortDirections.ContainsKey(header)) return;
            vm.SetSort(header, _sortDirections[header]);
            _sortDirections[header] = !_sortDirections[header];
            e.Handled = true;
        }
    }
}
