using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot;
using ScottPlot.Avalonia;
using UI.ViewModels;
namespace UI;

public partial class Canvas : UserControl
{
    public Canvas()
    {
        InitializeComponent();
        DataContext = new CanvasViewModel();

        double[] dataX = new double[] {};
        double[] dataY = new double[] {};

        AvaPlot LineGraph = this.Find<AvaPlot>("LineGraph");
        LineGraph.Plot.Add.Scatter(dataX, dataY);
        LineGraph.Plot.Axes.Left.Label.Text = "Vertical Axis Label";
        LineGraph.Plot.Axes.Bottom.Label.Text = "Horizontal Axis Label";
        LineGraph.Refresh();
    }
}