using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using UI.ViewModels;
using Avalonia.VisualTree;
using System.Linq;

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

    // to temporarily save and restore the scrollbar position upon update
    // (needs to somehow be connected to the sort method in the view model. Maybe we can move that logic here) 
    private void SaveScrollPosition()
    {
        var scrollViewer = MyDataGrid.GetVisualDescendants()
                                     .OfType<ScrollViewer>()
                                     .FirstOrDefault();
        if (scrollViewer != null)
        {
            _savedOffset = scrollViewer.Offset;
        }
    }

    private void RestoreScrollPosition()
    {
        var scrollViewer = MyDataGrid.GetVisualDescendants()
                                     .OfType<ScrollViewer>()
                                     .FirstOrDefault();
        if (scrollViewer != null)
        {
            scrollViewer.Offset = _savedOffset;
        }
    }
}