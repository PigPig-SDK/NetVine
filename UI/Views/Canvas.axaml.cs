using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Avalonia;
using System.ComponentModel.Design;
using UI.ViewModels;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;
    private AvaPlot _canvasPlot;

    public Canvas()
    {
        InitializeComponent();
        _vm = new CanvasViewModel(ChartService.Instance, ResourceService.Instance);
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

    private void DrawLineChart()
    {
        var history = _vm.SelectedResource switch
        {
            "CPU" => (_vm.CpuHistory, "CPU (%)"),
            "RAM" => (_vm.RamHistory, "RAM (MB)"),
            "Disk" => (_vm.DiskHistory, "Disk (%)"),
            "Network" => (_vm.NetworkHistory, "Network (%)"),
            _ => (_vm.CpuHistory, "CPU (%)")
        };

        if (history.Item1.Count == 0) return;

        var signal = _canvasPlot.Plot.Add.Signal(history.Item1.ToArray());
        signal.LegendText = history.Item2;

        // fix Y at 0-100 for percentages, auto scale for RAM in MB
        if (_vm.SelectedResource == "RAM")
            _canvasPlot.Plot.Axes.AutoScale();
        else
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, 100);
            _canvasPlot.Plot.Axes.AutoScaleX();
        }

        _canvasPlot.Plot.YLabel(history.Item2);
        _canvasPlot.Plot.ShowLegend();
    }
    private void DrawBarChart() { }
    private void DrawPieChart() { }
}