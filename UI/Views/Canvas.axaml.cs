using Avalonia.Controls;
using Core;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UI.ViewModels;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;
    private AvaPlot _canvasPlot;
    private List<string> _barProcessNames = new();
    private ScottPlot.Plottables.Annotation? _tooltip;
    private Dictionary<int, List<(string name, double yBase, double yTop)>> _barTooltipData = new();
    //add to settings
    int _topCount = 10;

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
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _canvasPlot = this.Find<AvaPlot>("CanvasPlot")!;
        _canvasPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#222228");
        _canvasPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D38");
        _canvasPlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#CCCCCC"));
        _canvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(0.1);
        _canvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        _canvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = false;
        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
        _canvasPlot.PointerMoved += OnPointerMoved;
        _canvasPlot.PointerExited += OnPointerExited;
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

        SetLimits();

        _canvasPlot.Plot.YLabel(history.Item2);
        _canvasPlot.Plot.ShowLegend();
    }

    private void DrawBarChart()
    {
        if (_vm.SnapshotHistory.Count == 0) return;

        _barTooltipData.Clear();

        var processColors = new Dictionary<string, ScottPlot.Color>();
        var palette = new ScottPlot.Palettes.Category10();
        int colorIndex = 0;

        for (int i = 0; i < _vm.SnapshotHistory.Count; i++)
        {
            var snapshot = _vm.SnapshotHistory[i];

            var bartop = snapshot
                .OrderByDescending(p => GetValue(p))
                .Where(p => GetValue(p) > 0)
                .Take(_topCount)
                .ToList();

            double cumulative = 0;
            var segmentData = new List<(string name, double yBase, double yTop)>();

            foreach (var process in bartop)
            {
                double value = GetValue(process);

                if (!processColors.ContainsKey(process.ProcessName))
                    processColors[process.ProcessName] = palette.GetColor(colorIndex++);

                var bar = new ScottPlot.Bar
                {
                    Position = i,
                    Value = cumulative + value,
                    ValueBase = cumulative,
                    FillColor = processColors[process.ProcessName],
                    LineColor = ScottPlot.Colors.Transparent,
                };

                _canvasPlot!.Plot.Add.Bar(bar);
                segmentData.Add((process.ProcessName, cumulative, cumulative + value));
                cumulative += value;
            }

            _barTooltipData[i] = segmentData;
        }

        SetLimits();

        string label = _vm.SelectedResource == ChartService.RAM ? "RAM (MB)" : "Usage (%)";
        _canvasPlot.Plot.YLabel(label);
    }
    private void DrawPieChart()
    {
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
            .Take(_topCount)
            .Where(p => GetValue(p) > 0)
            .ToList();
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_vm.CurrentChartType != ChartService.Bar || _canvasPlot is null) return;
        if (_barTooltipData.Count == 0) return;

        var pos = e.GetPosition(_canvasPlot);
        var dataCoords = _canvasPlot.Plot.GetCoordinates((float)pos.X, (float)pos.Y);

        int barIndex = (int)Math.Round(dataCoords.X);

        if (!_barTooltipData.TryGetValue(barIndex, out var segments))
        {
            RemoveTooltip();
            _canvasPlot.Refresh();
            return;
        }

        var hovered = segments.FirstOrDefault(s =>
            dataCoords.Y >= s.yBase && dataCoords.Y <= s.yTop);

        if (hovered == default)
        {
            RemoveTooltip();
            _canvasPlot.Refresh();
            return;
        }

        string unit = _vm.SelectedResource == ChartService.RAM ? "MB" : "%";
        double val = hovered.yTop - hovered.yBase;
        string text = $"{hovered.name}\n{val:0.0} {unit}";

        RemoveTooltip();
        _tooltip = _canvasPlot.Plot.Add.Annotation(text, Alignment.UpperLeft);
        _tooltip.LabelBackgroundColor = ScottPlot.Color.FromHex("#2D2D38");
        _tooltip.LabelFontColor = ScottPlot.Colors.White;
        _tooltip.LabelBorderColor = ScottPlot.Colors.White;
        _tooltip.LabelBorderWidth = 1;

        _canvasPlot.Refresh();
    }

    private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        RemoveTooltip();
        _canvasPlot?.Refresh();
    }

    private void RemoveTooltip()
    {
        if (_tooltip is null || _canvasPlot is null) return;
        _canvasPlot.Plot.Remove(_tooltip);
        _tooltip = null;
    }

    private void SetLimits()
    {
        if (_vm.SelectedResource == ChartService.RAM)
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, _vm.RAMTotal);
            _canvasPlot.Plot.Axes.AutoScaleX();
        }
        else
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, 100);
            _canvasPlot.Plot.Axes.AutoScaleX();
        }
    }

}