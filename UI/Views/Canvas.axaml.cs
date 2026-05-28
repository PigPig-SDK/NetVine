using Avalonia.Controls;
using Core;
using Infrastructure;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using UI.ViewModels;
using UI.Views;

namespace UI;

public partial class Canvas : UserControl
{
    private readonly CanvasViewModel _vm;
    private AvaPlot _canvasPlot;
    private List<string> _barProcessNames = new();
    private ScottPlot.Plottables.Annotation? _tooltip;
    private Dictionary<int, List<(string name, double yBase, double yTop)>> _barTooltipData = new();
    private bool _followFlag = true;
    private bool _dragFlag = false;
    private AxisLimits? _boundsSaved;

    private DateTime? HistoricalStartGraph = null;
    private DateTime? HistoricalEndGraph = null;
    
    //add to settings
    int _topCount = 10;
    private readonly Dictionary<string, ScottPlot.Color> _processColors = new();
    private readonly ScottPlot.Palettes.Category20 _palette = new();
    private int _colorIndex = 0;
    private int TopCount => ConfigManager.ReadSetting(SettingInt.TopCount) is int t && t > 0 ? t : 10;

    private bool IsLive => LiveViewModel.IsLive;
    private bool _timeSelected = false;
    private bool _showPlaceholder;
    public bool ShowPlaceholder
    {
        get => _showPlaceholder;
        set
        {
            if (_showPlaceholder == value) return;
            _showPlaceholder = value;
            SelectDataTag.IsVisible = _showPlaceholder;
        }
    }

    public Canvas()
    {
        InitializeComponent();
        _vm = new CanvasViewModel(ChartService.Instance, ResourceService.Instance);
        DataContext = _vm;
        
        _vm.ChartUpdateRequested += DrawChart;
        _vm.ChartTypeUpdateRequested += UpdateType;
        _vm.ChartUpdateRequested += UpdateMenu;

        
        MainWindowViewModel.OnTabChanged += UpdateGraphTimeFrame;
        LiveViewModel.ViewChangedEvent += LiveViewChanged;
        _canvasPlot = this.Find<AvaPlot>("CanvasPlot")!;
        LiveViewChanged(LiveViewModel.IsLive);
        SetColors();
        SetMenuForGraphs();
        _canvasPlot.PointerWheelChanged += (_, e) => { _followFlag = false; };
        _canvasPlot.PointerPressed += (_, e) => {_dragFlag = true;};
        _canvasPlot.PointerReleased += (_, e) => {_dragFlag = false;};
        _canvasPlot.PointerMoved += (_, e) => { if (_dragFlag) { _followFlag = false; }};
        _canvasPlot.Plot.Benchmark.IsVisible = false;

        Loaded += OnLoaded;
    }


    /// <summary>
    /// Called when "LIVE BUTTON" is flipped
    /// </summary>
    private void LiveViewChanged(bool islive)
    {
        ShowPlaceholder = !islive && !_timeSelected;
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        if(_vm.CurrentChartType == "Pie")
        {
            SetMenuForPieChart();
            return;
        }
        SetMenuForGraphs();
    }

    void SetMenuForGraphs()
    {
        _canvasPlot.Menu?.Clear();
        if (!IsLive) _canvasPlot.Menu?.Add("Select Timeframe", _ => OpenGraphTimeFrame());
        _canvasPlot.Menu?.Add("Follow Graph", _ => { _followFlag = true; DrawChart(); });
    }
    void SetMenuForPieChart()
    {
        _canvasPlot.Menu?.Clear();
        if(!IsLive) _canvasPlot.Menu?.Add("Select Timeframe", _ => OpenGraphTimeFrame());
    }

    void SetColors()
    {
        _canvasPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#2D2D38").WithAlpha(0);
        _canvasPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D38").WithAlpha(0);
        _canvasPlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#CCCCCC"));
        _canvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#FFFFFF").WithAlpha(0.1);
        _canvasPlot.Menu.Add("Select Timeframe", _ => OpenGraphTimeFrame());
        
        Loaded += OnLoaded;
        
    }
    
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _canvasPlot = this.Find<AvaPlot>("CanvasPlot")!;
        SetColors();
        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
        _canvasPlot.PointerMoved += OnPointerMoved;
        _canvasPlot.PointerExited += OnPointerExited;
        _canvasPlot.Refresh();
    }

    private void UpdateType()
    {
        _boundsSaved = null;
        _canvasPlot.Plot.Axes.AutoScale();
        _followFlag = true;
    }
    private void DrawChart()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            RemoveTooltip();
            _boundsSaved = _canvasPlot.Plot.Axes.GetLimits();
            
            _canvasPlot.Plot.Clear();
            _canvasPlot.Plot.Axes.SquareUnits(false);
            _canvasPlot.Plot.YLabel("");

            _canvasPlot.Plot.Axes.Top.IsVisible =       true;
            _canvasPlot.Plot.Axes.Right.IsVisible =     true;
            _canvasPlot.Plot.Axes.Bottom.IsVisible =    true;
            _canvasPlot.Plot.Axes.Left.IsVisible =      true;

            switch (_vm.CurrentChartType)
            {
                case "Line": DrawLineChart(); break;
                case "Bar": DrawBarChart(); break;
                case "Pie": DrawPieChart(); break;
            }

            if (!_followFlag &&  _boundsSaved != null)
            {
                _canvasPlot.Plot.Axes.SetLimits((AxisLimits) _boundsSaved);
            }
            
            _canvasPlot.Refresh();
        });
    }

    private void DrawLineChart()
    {
        _canvasPlot.UserInputProcessor.IsEnabled = true;
        SetGrid();

        var history = _vm.SelectedResource switch
        {
            ChartService.CPU => (data: _vm.CpuHistory, title: "CPU (%)"),
            ChartService.CPUHistory =>  (data: _vm.CpuAvgHistory, title: "CPU History (%)"),
            
            ChartService.RAM => (data: _vm.RamHistory, title: "RAM (MB)"),
            ChartService.RAMHistory => (data: _vm.RamAvgHistory, title: "RAM History (MB)"),
            
            ChartService.DISK => (data: _vm.DiskHistory, title: "Disk (MB/s)"),
            ChartService.DISKHistory => (data: _vm.DiskAvgHistory, title: "Disk History (MB/s)"),
            
            ChartService.NET => (data: _vm.NetworkHistory, title: "Network (MB/s)"),
            ChartService.NETHistory => (data: _vm.NetworkAvgHistory, title: "Network History (MB/s)"),
        
            _ => (data: _vm.CpuHistory, title: "CPU (%)")
        };

        if (history.data.Count == 0) return;

        var signal = _canvasPlot.Plot.Add.Signal(history.data.ToArray());
        signal.LegendText = history.title;
        signal.Color = ScottPlot.Colors.White;

        _canvasPlot.Plot.YLabel(history.title);
        _canvasPlot.Plot.ShowLegend();

        _canvasPlot.Plot.Axes.SetLimitsX(0, history.data.Count);
        SetLimits();
    }

    private void DrawBarChart()
    {
        
        SetGrid();
        var data = IsLive ? _vm.SnapshotHistory : _vm.AllHistorical;
        
        if (data.Count == 0) return;

        _barTooltipData.Clear();

        for (int i = 0; i < data.Count; i++)
        {
            
            var snapshot = data[i];
            
            var bartop = snapshot
                .OrderByDescending(p => GetValue(p))
                .Where(p => GetValue(p) > 0)
                .Take(TopCount)
                .ToList();

            double cumulative = 0;
            var segmentData = new List<(string name, double yBase, double yTop)>();

            foreach (var process in bartop)
            {
                
                double value = GetValue(process);

                if (!_processColors.ContainsKey(process.ProcessName))
                    _processColors[process.ProcessName] = _palette.GetColor(_colorIndex++);

                var bar = new ScottPlot.Bar
                {
                    Position = i,
                    Value = cumulative + value,
                    ValueBase = cumulative,
                    FillColor = _processColors[process.ProcessName],
                    LineColor = ScottPlot.Colors.Transparent,
                };

                _canvasPlot!.Plot.Add.Bar(bar);
                segmentData.Add((process.ProcessName, cumulative, cumulative + value));
                cumulative += value;
            }

            _barTooltipData[i] = segmentData;
        }

        var label = _vm.SelectedResource switch
        {
            ChartService.CPU => "CPU (%)",
            ChartService.CPUHistory => "CPU History (%)",
            
            ChartService.RAM => "RAM (MB)",
            ChartService.RAMHistory => "RAM History (MB)",
            
            ChartService.DISK => "Disk (MB/S)",
            ChartService.DISKHistory => "Disk History (MB/S)",
            
            ChartService.NET => "Network (MB/s)",
            ChartService.NETHistory => "Network History (MB/s)",
            _ => ""
        };

        _canvasPlot.Plot.YLabel(label);

        _canvasPlot.Plot.Axes.SetLimitsX(-0.5, data.Count + 0.5);
        SetLimits();
    }
    private void DrawPieChart() {
        _followFlag = true;
        _canvasPlot.Plot.Axes.SquareUnits(true);
        _canvasPlot.Plot.Grid.IsVisible = false;
        _canvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        _canvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = false;

        _canvasPlot.Plot.Axes.Top.IsVisible =   false;
        _canvasPlot.Plot.Axes.Right.IsVisible = false;
        _canvasPlot.Plot.Axes.Bottom.IsVisible =false;
        _canvasPlot.Plot.Axes.Left.IsVisible =  false;

        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;

        _canvasPlot.Plot.Axes.AutoScale();

        if (_vm.LatestSnapshot.Count == 0) return;

        var pietop = GetTopProcesses();
        if (pietop.Count == 0) return;

        double[] values = pietop.Select(p => (double)GetValue(p)).ToArray();

        var pie = _canvasPlot!.Plot.Add.Pie(values);

        for (int i = 0; i < pietop.Count; i++)
        {
            pie.Slices[i].FillColor = _palette.GetColor(i);
            pie.Slices[i].Label = "";
            pie.Slices[i].LegendText = $"{pietop[i].ProcessName} ({GetValue(pietop[i]):0.0})";
        }

        pie.LineColor = ScottPlot.Colors.White;
        pie.LineWidth = 2;
        pie.DonutFraction = 0.25;


        _canvasPlot.Plot.ShowLegend();
        _canvasPlot.Plot.Axes.AutoScale();
        _canvasPlot.Plot.Axes.SetLimits(-1.5, 1.5, -1.5, 1.5);
        _canvasPlot.Refresh();
    }
    private float GetValue(IProgramData p) => _vm.SelectedResource switch
    {
        ChartService.CPU => p.CpuUsage,
        ChartService.CPUHistory => p.CpuUsage,
        
        ChartService.RAM => p.MemoryUsage,
        ChartService.RAMHistory => p.MemoryUsage,
        
        ChartService.DISK => p.DiskUsage,
        ChartService.DISKHistory => p.DiskUsage,
        
        ChartService.NET => p.NetworkUsage,
        ChartService.NETHistory => p.NetworkUsage,
        _ => p.CpuUsage
    };

    private List<IProgramData> GetTopProcesses()
    {
        if (_vm.LatestSnapshot.Count == 0) return new();

        return _vm.LatestSnapshot
            .OrderByDescending(p => GetValue(p))
            .Take(TopCount)
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
        if (_vm.SelectedResource == ChartService.RAM || _vm.SelectedResource == ChartService.RAMHistory)
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, _vm.RAMTotal);
        }
        else if (_vm.SelectedResource == ChartService.CPU || _vm.SelectedResource == ChartService.CPUHistory)
        {
            _canvasPlot.Plot.Axes.SetLimitsY(0, 100);
        }
        else
        {
            _canvasPlot.Plot.Axes.AutoScaleY();
        }
    }
    
    private async void OpenGraphTimeFrame()
    {
        Debug.Log("OpenGraphTimeFrame called");
        if (LiveViewModel.IsLive || MainWindowViewModel.ActiveTab != MainWindowViewModel.GraphView) return;
            
        var timeFrameWindow = new TimeFrameSelectionWindow();

        if (Application.Current is null) return;
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
            
        (DateTime? date1, DateTime? date2)? dateRange = await timeFrameWindow.ShowDialog<(DateTime?, DateTime?)?>
            (mainWindow);
            
        if (!dateRange.HasValue)
        {
            return;
        }
            
        HistoricalStartGraph = dateRange.Value.date1;
        HistoricalEndGraph = dateRange.Value.date2;

        ShowPlaceholder = false;
        _timeSelected = true;

        ResourceService.Instance.TimeFrameUpdate(HistoricalStartGraph, HistoricalEndGraph);
            
        mainWindow.FindControl<FolderView>("FolderView")?.SetDateRange(HistoricalStartGraph, HistoricalEndGraph);
    }

    private void UpdateGraphTimeFrame(int x)
    {
        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow
            .FindControl<FolderView>("FolderView")?.SetDateRange(HistoricalStartGraph, HistoricalEndGraph);
    }

    private void SetGrid()
    {
        _canvasPlot.Plot.Grid.IsVisible = true;
        _canvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = true;
        _canvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = true;
        _canvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 2;
        _canvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 2;
    }

}