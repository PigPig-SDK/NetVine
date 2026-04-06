using Avalonia.Controls;
using Core;
using ScottPlot;
using ScottPlot.Avalonia;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UI.ViewModels;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;
    private AvaPlot _canvasPlot;

    //add to settings
    int topcount = 10;



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
        // TODO: make 0 the minimum x for graph
        // _canvasPlot.Plot.Axes.SetLimitsX(0, 100);


        _canvasPlot.Refresh();
    }

    private void DrawChart()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            _canvasPlot.Plot.Clear();

            _canvasPlot.Plot.YLabel("");
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
        _canvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(0.1);
        _canvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = true;
        _canvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = true;
        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 2;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 2;

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

        if (_vm.SelectedResource == ChartService.RAM)
            _canvasPlot.Plot.Axes.AutoScale();
        else
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, 100);
            _canvasPlot.Plot.Axes.AutoScaleX();
        }

        _canvasPlot.Plot.YLabel(history.Item2);
        _canvasPlot.Plot.ShowLegend();
    }
    private void DrawBarChart() {

    }
    private void DrawPieChart() {
        _canvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(1);
        _canvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        _canvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = false;
        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
        
        if (_vm.LatestSnapshot.Count == 0) return;

        var pietop = GetTopProcesses();
        if (pietop.Count == 0) return;


        if (pietop.Count == 0) return;

        double[] values = pietop.Select(p => (double)GetValue(p)).ToArray();

        var pie = _canvasPlot!.Plot.Add.Pie(values);

        for (int i = 0; i < pietop.Count; i++)
        {
            pie.Slices[i].Label = "";
            pie.Slices[i].LegendText = $"{pietop[i].ProcessName} ({GetValue(pietop[i]):0.0})";
        }

        
        _canvasPlot.Plot.ShowLegend();
        _canvasPlot.Plot.Axes.AutoScale();
    }
    private float GetValue(IProgramData p) => _vm.SelectedResource switch
    {
        ChartService.CPU => p.CpuUsage,
        ChartService.RAM => p.MemoryUsage,
        ChartService.DISK => p.DiskUsage,
        ChartService.NET => p.NetworkUsage,
        _ => p.CpuUsage
    };

    private List<IProgramData> GetTopProcesses()
    {
        if (_vm.LatestSnapshot.Count == 0) return new();

        return _vm.LatestSnapshot
            .OrderByDescending(p => GetValue(p))
            .Take(topcount)
            .Where(p => GetValue(p) > 0)
            .ToList();
    }

}