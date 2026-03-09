using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot;
namespace UI;

public partial class Canvas : UserControl
{
    public Canvas()
    {
        InitializeComponent();

        double[] dataX = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        double[] dataY = new double[] { -1, 2, 3, -4, 5, 6, -7, 8, 9, -10, 11, 12, -13 };
        
    }
}