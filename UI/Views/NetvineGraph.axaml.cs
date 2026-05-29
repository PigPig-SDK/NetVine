using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Core;
using Infrastructure;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using UI.ViewModels;
using UI.Views;

namespace UI;

public partial class NetvineGraph : UserControl
{
    private ScottPlot.Plottables.Annotation? _tooltip;
    private Dictionary<int, List<(string name, double yBase, double yTop)>> _barTooltipData = new();
    private bool _followFlag = true;
    private bool _dragFlag = false;
    private AxisLimits? _boundsSaved;
    private DateTime? HistoricalStartGraph = null;
    private DateTime? HistoricalEndGraph = null;
    private readonly Dictionary<string, Color> _processColors = new();
    private readonly ScottPlot.Palettes.Category20 _palette = new();
    private int _colorIndex = 0;
    public int GraphYMargin { get; set; } = 10;
    private int TopCount => ConfigManager.ReadSetting(SettingInt.TopCount) is int t && t > 0 ? t : 10;
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

    public NetvineGraph()
    {
        InitializeComponent();
        LiveViewChanged(LiveViewModel.IsLive);
        SetColors();
        SetMenuForGraphs();
        CanvasPlot.PointerWheelChanged += (_, e) => { _followFlag = false; };
        CanvasPlot.PointerPressed += (_, e) => {_dragFlag = true;};
        CanvasPlot.PointerReleased += (_, e) => {_dragFlag = false;};
        CanvasPlot.PointerMoved += (_, e) => { if (_dragFlag) { _followFlag = false; }};
        CanvasPlot.Plot.Benchmark.IsVisible = false;
        CanvasPlot.Plot.Axes.Top.IsVisible = false;
        CanvasPlot.Plot.Axes.Right.IsVisible = false;
        CanvasPlot.Plot.Axes.Bottom.IsVisible = false;
        CanvasPlot.Plot.Axes.SquareUnits(false);
        CanvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        CanvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        CanvasPlot.PointerMoved += OnPointerMoved;
        CanvasPlot.PointerExited += OnPointerExited;
        ChartService.Instance.ChartTypeChanged += UpdateChartType;
        ResourceService.Instance.DataUpdated += UpdateChart;
        MainWindowViewModel.OnTabChanged += UpdateGraphTimeFrame;
        LiveViewModel.ViewChangedEvent += LiveViewChanged;
        FolderViewData.OnSelectionUpdated += SelectionUpdated;
        DrawChart();//Force update.
        LiveViewChanged(LiveViewModel.IsLive);//Force update of live/historical view.
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CanvasPlot.PointerMoved -= OnPointerMoved;
        CanvasPlot.PointerExited -= OnPointerExited;
        ChartService.Instance.ChartTypeChanged -= UpdateChartType;
        ResourceService.Instance.DataUpdated -= UpdateChart;
        MainWindowViewModel.OnTabChanged -= UpdateGraphTimeFrame;
        LiveViewModel.ViewChangedEvent -= LiveViewChanged;
        FolderViewData.OnSelectionUpdated -= SelectionUpdated;

        base.OnDetachedFromVisualTree(e);
    }

    private void SelectionUpdated()
    {
        UpdateChart();
    }

    private void UpdateChart()
    {
        DrawChart();
        UpdateMenu();
    }

    private void UpdateChartType()
    {
        UpdateMenu();
        DrawChart();
        UpdateType();
    }
    /// <summary>
    /// Called when "LIVE BUTTON" is flipped
    /// </summary>
    private void LiveViewChanged(bool islive)
    {
        ShowPlaceholder = !islive && !_timeSelected;
        UpdateMenu();
        DrawChart();
    }

    private void UpdateMenu()
    {
        if(ChartService.Instance.ChartType == ChartService.Pie)
        {
            SetMenuForPieChart();
            return;
        }
        SetMenuForGraphs();
    }

    void SetMenuForGraphs()
    {
        CanvasPlot.Menu?.Clear();
        if (!LiveViewModel.IsLive) CanvasPlot.Menu?.Add("Select Timeframe", _ => OpenGraphTimeFrame());
        CanvasPlot.Menu?.Add("Follow Graph", _ => { _followFlag = true; DrawChart(); });
    }
    void SetMenuForPieChart()
    {
        CanvasPlot.Menu?.Clear();
        if(!LiveViewModel.IsLive) CanvasPlot.Menu?.Add("Select Timeframe", _ => OpenGraphTimeFrame());
    }

    void SetColors()
    {
        CanvasPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#2D2D38").WithAlpha(0);//Transparent
        CanvasPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#2D2D38").WithAlpha(0);//Transparent
        CanvasPlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#CCCCCC"));
        CanvasPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#2A2A2A");
    }

    private void UpdateType()
    {
        _boundsSaved = null;
        CanvasPlot.Plot.Axes.AutoScale();
        _followFlag = true;
    }
    private void DrawChart()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            RemoveTooltip();
            _boundsSaved = CanvasPlot.Plot.Axes.GetLimits();
            
            CanvasPlot.Plot.Clear();
            CanvasPlot.Plot.Axes.Left.IsVisible = true;

            switch (ChartService.Instance.ChartType)
            {
                case ChartService.Line: DrawLineChart(); break;
                case ChartService.Bar: DrawBarChart(); break;
                case ChartService.Pie: DrawPieChart(); break;
            }

            if (!_followFlag &&  _boundsSaved != null)
                CanvasPlot.Plot.Axes.SetLimits((AxisLimits) _boundsSaved);

            var label = ChartService.Instance.SelectedResource switch
            {
                ResourceTypes.CPU => "CPU (%)",
                ResourceTypes.RAM => "RAM (MB)",
                ResourceTypes.Disk => "Disk (MB/S)",
                ResourceTypes.Network => "Network (MB/s)",
                _ => ""
            };
            CanvasPlot.Plot.YLabel(label);
            CanvasPlot.Refresh();
        });
    }

    #region LineChart
    private void DrawLineChart()
    {
        if(LiveViewModel.IsLive)
            DrawLiveLineChart();
        else
            DrawHistoryLineChart();
    }
    private void DrawHistoryLineChart()
    {
        SetGrid();

        foreach (var user in FolderViewData.SelectedUsersWithIndex().ToArray())
        {
            Debug.Log(ChartService.Instance.SelectedResource.ToString());

            var history = ResourceService.Instance.GetDatedResourceForUser(ChartService.Instance.SelectedResource, user.name);
            if (history.data.Count == 0) continue;

            var timestamps = history.timeStamps;
            var data = history.data;

            var paddedDates = new List<double>();
            var paddedData = new List<double>();

            var threshold = TimeSpan.FromSeconds(90);

            for (int i = 0; i < timestamps.Count; i++)
            {
                if (i > 0 && timestamps[i] - timestamps[i - 1] > threshold)
                {
                    paddedDates.Add(timestamps[i - 1].AddSeconds(1).ToOADate());
                    paddedData.Add(double.NaN);
                }
                paddedDates.Add(timestamps[i].ToOADate());
                paddedData.Add(data[i] == 0 ? double.NaN : data[i]);
            }

            var scatter = CanvasPlot.Plot.Add.Scatter(
                paddedDates.ToArray(),
                paddedData.ToArray()
            );
            scatter.LegendText = history.title;
            scatter.Color = _palette.GetColor(user.index);
            scatter.MarkerSize = 0;
        }
        CanvasPlot.Plot.Axes.AutoScaleX();
        CanvasPlot.Plot.ShowLegend();
        SetLimits();
    }

    private void DrawLiveLineChart()
    {
        SetGrid();
        int historyMax = 0;

        // First pass - find the true max length
        var histories = new List<(double[] data, (string title, int index))>();
        foreach (var user in FolderViewData.SelectedUsersWithIndex().ToArray())
        {
            var history = ResourceService.Instance.GetResourceForUser(ChartService.Instance.SelectedResource, user.name);
            if (history.data.Count == 0) continue;
            histories.Add((history.data.ToArray(), (history.title,user.index)));
            historyMax = (int)MathF.Max(history.data.Count, historyMax);
        }

        //Second pass - plot the data with padding to align to the right
        foreach (var (data, displayData) in histories)
        {
            double[] padded = new double[historyMax];
            int offset = historyMax - data.Length;
            for (int i = 0; i < offset; i++)
                padded[i] = 0;
            Array.Copy(data, 0, padded, offset, data.Length);

            //display
            var signal = CanvasPlot.Plot.Add.Signal(padded);
            signal.LegendText = displayData.title;
            signal.Color = _palette.GetColor(displayData.index);
        }

        CanvasPlot.Plot.ShowLegend();
        CanvasPlot.Plot.Axes.SetLimitsX(0, historyMax);
        SetLimits();
    }
    #endregion

    #region BarChart
    private void DrawBarChart()
    {
        SetGrid();
        _barTooltipData.Clear();

        int historyMax = 0;
        var users = FolderViewData.SelectedUsersWithIndex().ToArray();
        foreach (var user in users)
        {
            var history = ResourceService.Instance.GetProgramUsageForUser(user.name);
            historyMax = (int)MathF.Max(history.Count, historyMax);
        }

        var mergedPool = new List<IProgramData>[historyMax].Select(_ => new List<IProgramData>()).ToArray();
        foreach (var user in users)
        {
            var history = ResourceService.Instance.GetProgramUsageForUser(user.name);
            if (history.Count == 0) continue;
            int offset = historyMax - history.Count;
            for (int i = 0; i < history.Count; i++)
                mergedPool[offset + i].AddRange(history[i]);
        }

        for (int i = 0; i < historyMax; i++)
        {
            if (mergedPool[i].Count == 0) continue;

            var topPrograms = mergedPool[i]
                .GroupBy(x => x.ProcessName)
                .OrderByDescending(g => g.Sum(x => GetValue(x)))
                .Take(TopCount)
                .ToList();

            double cumulative = 0;
            var segmentData = new List<(string name, double yBase, double yTop)>();

            foreach (var group in topPrograms)
            {
                double value = group.Sum(x => GetValue(x));
                if (!_processColors.ContainsKey(group.Key))
                    _processColors[group.Key] = _palette.GetColor(_colorIndex++);

                CanvasPlot.Plot.Add.Bar(new ScottPlot.Bar
                {
                    Position = i,
                    Value = cumulative + value,
                    ValueBase = cumulative,
                    FillColor = _processColors[group.Key],
                    LineColor = ScottPlot.Colors.Transparent,
                });

                segmentData.Add((group.Key, cumulative, cumulative + value));
                cumulative += value;
            }

            _barTooltipData[i] = segmentData;
        }

        CanvasPlot.Plot.Axes.SetLimitsX(-0.5, historyMax + 0.5);
        SetLimits();
    }
    #endregion
    #region PieChart
    private void DrawPieChart() {
        return;

        _followFlag = true;
        CanvasPlot.Plot.Axes.SquareUnits(true);
        CanvasPlot.Plot.Grid.IsVisible = false;
        CanvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        CanvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = false;
        CanvasPlot.Plot.Axes.Left.IsVisible =  false;

        CanvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        CanvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 0;

        CanvasPlot.Plot.Axes.AutoScale();

        var pietop = GetTopProcesses();
        if (pietop.Count == 0) return;

        double[] values = pietop.Select(p => (double)GetValue(p)).ToArray();

        var pie = CanvasPlot!.Plot.Add.Pie(values);

        for (int i = 0; i < pietop.Count; i++)
        {
            pie.Slices[i].FillColor = _palette.GetColor(i);
            pie.Slices[i].Label = "";
            pie.Slices[i].LegendText = $"{pietop[i].ProcessName} ({GetValue(pietop[i]):0.0})";
        }

        pie.LineColor = ScottPlot.Colors.White;
        pie.LineWidth = 2;
        pie.DonutFraction = 0.25;


        CanvasPlot.Plot.ShowLegend();
        CanvasPlot.Plot.Axes.AutoScale();
        CanvasPlot.Plot.Axes.SetLimits(-1.5, 1.5, -1.5, 1.5);
        CanvasPlot.Refresh();
    }
    #endregion
    private float GetValue(IProgramData p) => ChartService.Instance.SelectedResource switch
    {
        ResourceTypes.CPU => p.CpuUsage,
        ResourceTypes.RAM => p.MemoryUsage,
        ResourceTypes.Disk => p.DiskUsage,
        ResourceTypes.Network => p.NetworkUsage,
        _ => p.CpuUsage
    };

    private List<IProgramData> GetTopProcesses()
    {
        return new();

        //return ResourceService.Instance.LatestSnapshot
        //    .OrderByDescending(p => GetValue(p))
        //    .Take(TopCount)
        //    .Where(p => GetValue(p) > 0) 
        //    .ToList();
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (ChartService.Instance.ChartType != ChartService.Bar || CanvasPlot is null) return;
        if (_barTooltipData.Count == 0) return;

        var pos = e.GetPosition(CanvasPlot);
        var dataCoords = CanvasPlot.Plot.GetCoordinates((float)pos.X, (float)pos.Y);

        int barIndex = (int)Math.Round(dataCoords.X);

        if (!_barTooltipData.TryGetValue(barIndex, out var segments))
        {
            RemoveTooltip();
            CanvasPlot.Refresh();
            return;
        }

        var hovered = segments.FirstOrDefault(s =>
            dataCoords.Y >= s.yBase && dataCoords.Y <= s.yTop);

        if (hovered == default)
        {
            RemoveTooltip();
            CanvasPlot.Refresh();
            return;
        }

        double val = hovered.yTop - hovered.yBase;
        string text = $"{hovered.name}\n{val:0.0} {ActiveUnit()}";

        RemoveTooltip();
        _tooltip = CanvasPlot.Plot.Add.Annotation(text, Alignment.UpperLeft);
        _tooltip.LabelFontColor = ScottPlot.Colors.White;
        _tooltip.LabelBorderColor = ScottPlot.Colors.White;
        _tooltip.LabelBorderWidth = 1;

        CanvasPlot.Refresh();
    }

    public string ActiveUnit()
    {
        switch(ChartService.Instance.SelectedResource)
        {
            case ResourceTypes.CPU:
            return "%";
            case ResourceTypes.RAM:
                return "MB";
            case ResourceTypes.Disk:
                return "MB/s";
            case ResourceTypes.Network:
                return "MB/s";
            default:
            return "";
        }
    }

    private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        RemoveTooltip();
        CanvasPlot?.Refresh();
    }

    private void RemoveTooltip()
    {
        if (_tooltip is null || CanvasPlot is null) return;
        CanvasPlot.Plot.Remove(_tooltip);
        _tooltip = null;
    }

    private void SetLimits()
    {
        if (ChartService.Instance.SelectedResource == ResourceTypes.CPU)
        {
            CanvasPlot.Plot.Axes.SetLimitsY(0 - GraphYMargin, 100 + GraphYMargin);
        }
        else
        {
            CanvasPlot.Plot.Axes.AutoScaleY();
        }
    }
    
    private async void OpenGraphTimeFrame()
    {
        if (LiveViewModel.IsLive || MainWindowViewModel.ActiveTab != MainWindowViewModel.GraphView) return;
            
        var timeFrameWindow = new TimeFrameSelectionWindow();

        if (Application.Current is null) return;
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
            
        (DateTime? date1, DateTime? date2)? dateRange = await timeFrameWindow.ShowDialog<(DateTime?, DateTime?)?>
            (mainWindow);
            
        if (!dateRange.HasValue)
            return;
            
        HistoricalStartGraph = dateRange.Value.date1;
        HistoricalEndGraph = dateRange.Value.date2;

        ShowPlaceholder = false;
        _timeSelected = true;
        _followFlag = true;

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
        CanvasPlot.Plot.Grid.IsVisible = true;
        CanvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = true;
        CanvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = true;
        CanvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 2;
        CanvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 2;
    }

}