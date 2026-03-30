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
        if (_vm.CpuHistory.Count == 0) return;

        var cpu = CanvasPlot.Plot.Add.Signal(_vm.CpuHistory.ToArray());
        cpu.LegendText = "CPU (%)";
        cpu.Color = ScottPlot.Color.FromHex("#FF6B6B");

        var ram = CanvasPlot.Plot.Add.Signal(_vm.RamHistory.ToArray());
        ram.LegendText = "RAM (MB)";
        ram.Color = ScottPlot.Color.FromHex("#4ECDC4");

        var disk = CanvasPlot.Plot.Add.Signal(_vm.DiskHistory.ToArray());
        disk.LegendText = "Disk (MB/s)";
        disk.Color = ScottPlot.Color.FromHex("#FFE66D");

        var network = CanvasPlot.Plot.Add.Signal(_vm.NetworkHistory.ToArray());
        network.LegendText = "Network (MB/s)";
        network.Color = ScottPlot.Color.FromHex("#A5C882");

        CanvasPlot.Plot.ShowLegend();
        CanvasPlot.Plot.YLabel("Usage");
        CanvasPlot.Plot.XLabel("Time (samples)");
    }
    private void DrawBarChart() { }
    private void DrawPieChart() { }
}