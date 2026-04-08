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
        LiveViewModel.ViewChangedEvent += OnViewChanged;
        LiveViewModel.ViewChangedEvent += TimeFrameDisableOnLive;
    }

    /// <summary>
    /// Prevents program from crashing when changing between live view and historical
    /// </summary>
    /// <param name="isLive"></param>
    private void OnViewChanged(bool isLive)
    {
        MyDataGrid.SelectedItem = null;
    }

    private void DataGridLoaded(object? sender, RoutedEventArgs e)
    {
        foreach (var column in MyDataGrid.Columns)
            _sortDirections[column.Header?.ToString() ?? ""] = true;
    }

    private void DataGridSorting(object? sender, DataGridColumnEventArgs e)
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

    private void TimeFrameDisableOnLive(bool isLive)
    {
        if (isLive)
        {
            TimeFrameSelectionOption.IsEnabled = false;
        }
        else
        {
            TimeFrameSelectionOption.IsEnabled = true;
        }
    }
}
