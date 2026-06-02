using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI.ViewModels;
using UI.Views;

namespace UI;

public partial class TableView : UserControl
{
    private Dictionary<string, bool> _sortDirections = new();
    private TableRow? _selectedRow;

    public TableView()
    {
        InitializeComponent();
        DataContext = new TableViewModel();
        if (DataContext is TableViewModel vm)
        {
            vm.TableData.ClearSelection = () => MyDataGrid.SelectedItem = null;
            vm.ClearSelection = () => MyDataGrid.SelectedItem = null;
            vm.OnLoadingUpdated += LoadingUpdated;
        }
        LiveViewModel.ViewChangedEvent += OnViewChanged;
        LiveViewModel.ViewChangedEvent += TimeFrameDisableOnLive;
        

        MyDataGrid.LoadingRow += RecolorRows;
    }

    private void LoadingUpdated(bool isLoading)
    {
        Dispatcher.UIThread.Post(() => LoadingBar.IsVisible = isLoading);
    }

    private void SetupSearchTooltip()
    {
        var mainWindowBase = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if(mainWindowBase is MainWindow mainWindow)
        {
            mainWindow.SetSearchTooltip("Boolean Operators:" +
                "\nAnd : &\nOr : +" +
                "\nEquality : <,>,<=,>=,=" +
                "\nParentheses, '()' are respected." +
                "\nQuery Properties:" +
                "\nprocess, system, cpu, disk, memory, network, cpuavg, diskavg, memoryavg, networkavg, cpupeak, diskpeak, memorypeak, networkpeak");
        }
    }

    private void RecolorRows(object? sender, DataGridRowEventArgs e)
    {
        e.Row.PropertyChanged += (s, args) =>
        {
            if (args.Property == DataGridRow.IsSelectedProperty)
            {
                e.Row.Background = e.Row.IsSelected
                    ? new SolidColorBrush(Color.Parse("#8FB56A"))
                    : Brushes.Transparent;

                foreach (var cell in e.Row.GetVisualDescendants().OfType<DataGridCell>())
                {
                    cell.Background = e.Row.IsSelected
                        ? new SolidColorBrush(Color.Parse("#8FB56A"))
                        : Brushes.Transparent;
                }
            }
        };
    }

    private void Animate(double time)
    {
        if(DataContext is TableViewModel vm && vm.IsLoading)
        {
            LoadingBar.Animate(time);
        }
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is TableViewModel vm)
        {
            vm.ToggleEvents(true);
        }
        MainWindowViewModel.OnAnimateFrame += Animate;
        SetupSearchTooltip();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is TableViewModel vm)
        {
            vm.ToggleEvents(false);
        }
        MainWindowViewModel.OnAnimateFrame -= Animate;
        base.OnDetachedFromVisualTree(e);
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
        AppQuitter.KillProcessesByRowAsync(_selectedRow);

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
