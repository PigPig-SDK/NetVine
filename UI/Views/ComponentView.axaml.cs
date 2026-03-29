using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.ViewModels;

namespace UI;

public partial class ComponentView : UserControl
{
    public ComponentView()
    {
        InitializeComponent();
        DataContext = new ComponentViewModel(ChartService.Instance);
    }
}