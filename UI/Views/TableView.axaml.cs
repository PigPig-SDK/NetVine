using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI.ViewModels;

namespace UI;

public partial class TableView : UserControl
{
    private Dictionary<string, bool> _sortDirections = new();
    private TableRow? _selectedRow;

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
        EndProgramMenuItem.IsEnabled = isLive;
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
        _selectedRow = MyDataGrid.SelectedItem as TableRow;

        if (DataContext is TableViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.SearchText))
            {
                //turn off update temporarily to ensure context menu stays open

                vm.PauseUpdate();

            }
        }
    }
    private void OnEndProgramClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableViewModel vm)
            vm.ResumeUpdate();
        if (_selectedRow == null) return;//Don't do anything.
        _ = AppQuitter.KillProcessesByRowAsync(_selectedRow);

    }

    private void ContextMenuClosed(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableViewModel vm)
            vm.ResumeUpdate();
    }

    private async void OnSaveIconClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TableViewModel vm)
            vm.ResumeUpdate();
        if (_selectedRow == null) return;//Don't do anything.

        var topLevel = TopLevel.GetTopLevel(this);
        if(topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Your Icon",
            SuggestedFileName = "icon.png",
            DefaultExtension = "png",
            FileTypeChoices = new[]
            {
            new FilePickerFileType("Icon") { Patterns = new[] { "*.png" } }
        }
        });

        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        using var writer = new StreamWriter(stream);
        _selectedRow.AppIcon?.Save(stream, 100);
    }
}
