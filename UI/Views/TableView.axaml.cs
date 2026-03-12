using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using UI.ViewModels;

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

    private void DataGrid_Sorting_1(object? sender, DataGridColumnEventArgs e)
    {
    }
}