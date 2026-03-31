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
        _canvasPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#222228");
        _canvasPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D38");
        _canvasPlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#CCCCCC"));
        _canvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(0.1);
        // TODO: make 0 the minimum x for graph
        // _canvasPlot.Plot.Axes.SetLimitsX(0, 100);
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
            ChartService.CPU => (_vm.CpuHistory, "CPU (%)"),
            ChartService.RAM => (_vm.RamHistory, "RAM (MB)"),
            ChartService.DISK => (_vm.DiskHistory, "Disk (%)"),
            ChartService.NET => (_vm.NetworkHistory, "Network (MB/s)"),
            _ => (_vm.CpuHistory, "CPU (%)")
        };

        if (history.Item1.Count == 0) return;

        var signal = _canvasPlot.Plot.Add.Signal(history.Item1.ToArray());
        signal.LegendText = history.Item2;
        signal.Color = ScottPlot.Colors.White;

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