using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Avalonia;
using System.ComponentModel.Design;
using UI.ViewModels;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;

    public Canvas()
    {
        InitializeComponent();
        _vm = new CanvasViewModel(ChartService.Instance, ResourceService.Instance);
        DataContext = _vm;
        _vm.ChartUpdateRequested += DrawChart;
        CanvasPlot = this.Find<AvaPlot>("CanvasPlot")!;
        CanvasPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#222228");
        CanvasPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D38");
        CanvasPlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#CCCCCC"));
        CanvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(0.1);
        // TODO: make 0 the minimum x for graph
        // _canvasPlot.Plot.Axes.SetLimitsX(0, 100);
        CanvasPlot.Refresh();
        CanvasPlot.UserInputProcessor.Disable();
    }

    private void DrawChart()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            CanvasPlot.Plot.Clear();

            switch (_vm.CurrentChartType)
            {
                case "Line": DrawLineChart(); break;
                case "Bar": DrawBarChart(); break;
                case "Pie": DrawPieChart(); break;
            }

            CanvasPlot.Refresh();
        });
    }

    private void DrawLineChart()
    {
        (System.Collections.Generic.List<double> data, string title) history = _vm.SelectedResource switch
        {
            ChartService.CPUChartname => (_vm.CpuHistory, "CPU (%)"),
            ChartService.RAMChartName => (_vm.RamHistory, "RAM (MB)"),
            ChartService.DiskChartName => (_vm.DiskHistory, "Disk (%)"),
            ChartService.NetworkChartName => (_vm.NetworkHistory, "Network (MB/s)"),
            _ => (_vm.CpuHistory, "CPU (%)")
        };

        if (history.data.Count == 0) return;

        var signal = CanvasPlot.Plot.Add.Signal(history.data.ToArray());
        signal.LegendText = history.title;
        signal.Color = ScottPlot.Colors.White;

        if (_vm.SelectedResource == ChartService.RAMChartName)
            CanvasPlot.Plot.Axes.AutoScale();
        else
        {
            CanvasPlot.Plot.Axes.SetLimitsY(0, 100);
            CanvasPlot.Plot.Axes.AutoScaleX();
        }

        CanvasPlot.Plot.YLabel(history.title);
        CanvasPlot.Plot.ShowLegend();
    }
    private void DrawBarChart() { }
    private void DrawPieChart() { }
}