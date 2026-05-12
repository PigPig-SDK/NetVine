using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.ViewModels;

namespace UI;

public partial class LogView : UserControl
{
    public LogView()
    {


        InitializeComponent();
        var vm = new LogViewModel();
        DataContext = vm;


    }


}