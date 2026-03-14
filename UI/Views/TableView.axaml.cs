using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using UI.ViewModels;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Interactivity;
using System;

namespace UI;

public partial class TableView : UserControl
{
    public TableView()
    {
        InitializeComponent();
        DataContext = new TableViewModel();
    }

    private void DataGrid_Sorting(object? sender, DataGridColumnEventArgs e)
    {
        Console.WriteLine($"Header type: {e.Column.Header?.GetType()}, value: {e.Column.Header}");
        if (DataContext is TableViewModel vm)
        {
            vm.SetSort(e.Column.Header?.ToString());
            e.Handled = true;
        }
    }

    private void GearButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private Vector _savedOffset;

    private ScrollViewer? _scrollViewer;

    private void DataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        _scrollViewer = MyDataGrid.GetVisualDescendants()
                                  .OfType<ScrollViewer>()
                                  .FirstOrDefault();

        if (_scrollViewer != null)
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

        if (DataContext is TableViewModel vm)
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TableViewModel.FilteredRows))
                    Avalonia.Threading.Dispatcher.UIThread.Post(RestoreScrollPosition,
                        Avalonia.Threading.DispatcherPriority.Loaded);
            };
    }

    private void SaveScrollPosition()
    {
        if (_scrollViewer != null)
            _savedOffset = _scrollViewer.Offset;
    }

    private bool _isRestoring = false;

    private void RestoreScrollPosition()
    {
        if (_scrollViewer != null)
        {
            _isRestoring = true;
            _scrollViewer.Offset = _savedOffset;
            // don't set _isRestoring = false here, let ScrollViewer_ScrollChanged reset it
        }
    }

    private void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isRestoring)
        {
            _isRestoring = false; // reset flag after the restore scroll event fires
            return;
        }
        SaveScrollPosition();
    }
}