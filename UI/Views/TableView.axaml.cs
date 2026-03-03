using Avalonia;
using Avalonia.Controls;
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

    private void GearButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}