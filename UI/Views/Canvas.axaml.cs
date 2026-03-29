using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Avalonia;
using System.ComponentModel.Design;
using UI.ViewModels;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;
    private AvaPlot _canvasPlot; // field so all methods can access it

    public Canvas()
    {
        InitializeComponent();
        _vm = new CanvasViewModel(ChartService.Instance);
        DataContext = _vm;
        _vm.ChartUpdateRequested += DrawChart;
        _canvasPlot = this.Find<AvaPlot>("CanvasPlot")!;
        _canvasPlot.Refresh();
    }

    private void DrawChart()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            _canvasPlot.Plot.Clear();

            switch (_vm.CurrentChartType)
            {
                case "Line": DrawLineChart(); break;
                case "Bar": DrawBarChart(); break;
                case "Pie": DrawPieChart(); break;
            }

            _canvasPlot.Refresh();
        });
    }

    private void DrawLineChart() { }
    private void DrawBarChart() { }
    private void DrawPieChart() { }
}