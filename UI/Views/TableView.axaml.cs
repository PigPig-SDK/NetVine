using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UI.ViewModels;

namespace UI;

public partial class TableView : UserControl
{
    private Dictionary<string, bool> _sortDirections = new();
    private Tuple<string, string>? _selectedRowKey = null;



    public TableView()
    {
        InitializeComponent();
        DataContext = new TableViewModel();
        if (DataContext is TableViewModel vm)
            vm.TableData.ClearSelection = () => MyDataGrid.SelectedItem = null;
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

    private void ContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        var selectedRow = MyDataGrid.SelectedItem as TableRow;
        if (selectedRow == null) return;

        _selectedRowKey = new Tuple<string, string>(selectedRow.SystemName, selectedRow.AppName);

        if (DataContext is TableViewModel vm)
            vm.PauseUpdate();
    }


    private void OnEndProgramClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableViewModel vm)
        {
            vm.ResumeUpdate();

            if (_selectedRowKey == null) return;
            _ = vm.TableData.KillAndRemoveByKey(_selectedRowKey.Item1, _selectedRowKey.Item2);
        }
    }

    private void ContextMenuClosed(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableViewModel vm)
            vm.ResumeUpdate();
    }
}
