using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Core;
using Infrastructure;
using Infrastructure.Notifications;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UI.ViewModels;
using UI.Views;


namespace UI;

public partial class NetvineGraph : UserControl
{
    public ObservableCollection<GraphLedgendEntry> LogEntries { get; } = new();
    private Dictionary<double, List<(string name, string programName, double yBase, double yTop)>> _barTooltipData = new();
    private bool _followFlag = true;
    private bool _dragFlag = false;
    private AxisLimits? _boundsSaved;
    private DateTime? HistoricalStartGraph = null;
    private DateTime? HistoricalEndGraph = null;
    private readonly Dictionary<string, ScottPlot.Color> _processColors = new();
    private readonly PastelPalette _palette = new();
    private int _colorIndex = 0;
    public int GraphYMargin { get; set; } = 10;
    private int TopCount => ConfigManager.ReadSetting(SettingInt.TopCount) is int t && t > 0 ? t : 10;
    private bool _timeSelected = false;
    private bool _showPlaceholder;
    private bool _isLoading = false;
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
        DataContext = this;
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
        CanvasPlot.Plot.HideLegend();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        CanvasPlot.PointerMoved += OnPointerMoved;
        CanvasPlot.PointerExited += OnPointerExited;
        ChartService.Instance.ChartTypeChanged += UpdateChartType;
        ResourceService.Instance.DataUpdated += UpdateChart;
        ResourceService.Instance.LoadingStatusChanged += LoadingStatusChanged;
        MainWindowViewModel.OnTabChanged += UpdateGraphTimeFrame;
        LiveViewModel.ViewChangedEvent += LiveViewChanged;
        FolderViewData.OnSelectionUpdated += SelectionUpdated;
        MainWindowViewModel.OnAnimateFrame += AnimationFrame;
        DrawChart();//Force update.
        LiveViewChanged(LiveViewModel.IsLive);//Force update of live/historical view.
        SetCombinationLock(true);
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
        MainWindowViewModel.OnAnimateFrame -= AnimationFrame;
        ResourceService.Instance.LoadingStatusChanged -= LoadingStatusChanged;
        SetCombinationLock(false);
        base.OnDetachedFromVisualTree(e);
    }

    private void LoadingStatusChanged(bool isLoading)
    {
        _isLoading = isLoading;
        LoadingBar.IsVisible = isLoading;
    }

    private void AnimationFrame(double time)
    {
        if (!_isLoading) return;//Only animate when loading to save resources.

        LoadingCircle.StrokeDashOffset = (time * 8) + MathF.Sin((float)time*2)*10;
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
            CanvasPlot.Plot.Axes.SquareUnits(false);
            CanvasPlot.Plot.Clear();
            CanvasPlot.Plot.Axes.Left.IsVisible = true;
            CanvasPlot.Plot.Grid.IsVisible = true;

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
        LogEntries.Clear();
        var users = FolderViewData.SelectedUsersWithIndex().ToArray();FolderViewData.SelectedUsersWithIndex().ToArray();

        foreach (var user in users)
        {
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
            LogEntries.Add(new GraphLedgendEntry() { Title = scatter.LegendText, Color = new SolidColorBrush(new Avalonia.Media.Color(scatter.Color.Alpha, scatter.Color.R, scatter.Color.G, scatter.Color.B)) });
        }
        CanvasPlot.Plot.Axes.AutoScaleX();
        SetLimits();
    }

    private void DrawLiveLineChart()
    {
        SetGrid();
        LogEntries.Clear();
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
            LogEntries.Add(new GraphLedgendEntry() { Title = signal.LegendText, Color = new SolidColorBrush(new Avalonia.Media.Color(signal.Color.Alpha, signal.Color.R, signal.Color.G, signal.Color.B)) });
        }
        CanvasPlot.Plot.Axes.SetLimitsX(0, historyMax);
        SetLimits();
    }
    #endregion

    #region BarChart
    private void DrawBarChart()
    {
        LogEntries.Clear();
        if (LiveViewModel.IsLive)
            DrawBarChartLive();
        else
            DrawBarChartHistorical();
    }
    private void DrawBarChartLive()
    {
        SetGrid();
        _barTooltipData.Clear();

        int historyMax = 0;
        var users = FolderViewData.SelectedUsersWithIndex().ToArray();
        //Find maximum to add buffer, so bars allign
        foreach (var user in users)
        {
            var history = ResourceService.Instance.GetProgramUsageForUser(user.name);
            historyMax = (int)MathF.Max(history.Count, historyMax);
        }

        var mergedPool = new List<(IProgramData data, string user)>[historyMax]
            .Select(_ => new List<(IProgramData data, string user)>())
            .ToArray();

        foreach (var user in users)
        {
            var history = ResourceService.Instance.GetProgramUsageForUser(user.name);
            if (history.Count == 0) continue;
            int offset = historyMax - history.Count;
            for (int i = 0; i < history.Count; i++)
                mergedPool[offset + i].AddRange(history[i].Select(p => (p.DeepCopy(), user.name)));
        }

        for (int i = 0; i < historyMax; i++)
        {
            if (mergedPool[i].Count == 0) continue;

            var topPrograms = mergedPool[i]
                .GroupBy(x => $"{x.data.ProcessName} ({x.user})")
                .OrderByDescending(g => g.Sum(x => GetActiveResourceValue(x.data)))
                .Take(TopCount)
                .ToList();

            double cumulative = 0;
            var segmentData = new List<(string name, string, double yBase, double yTop)>();

            foreach (var group in topPrograms)
            {
                double value = group.Sum(x => GetActiveResourceValue(x.data));
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

                segmentData.Add((group.Key, group.FirstOrDefault().data.ProcessName, cumulative, cumulative + value));
                cumulative += value;
            }

            _barTooltipData[i] = segmentData;
        }

        CanvasPlot.Plot.Axes.AutoScaleX();
        SetLimits();
    }
    private static DateTime FloorToMinute(DateTime dt, int minutes = 1)
    => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / minutes) * minutes, 0);
    private void DrawBarChartHistorical()
    {
        SetGrid();
        _barTooltipData.Clear();
        _processColors.Clear();
        _colorIndex = 0;

        var users = FolderViewData.SelectedUsersWithIndex().ToArray();

        // Collect all data bucketed by minute
        var allEntries = users
            .Select(user => (user, history: ResourceService.Instance.GetDatedProgramUsageForUser(user.name)))
            .Where(x => x.history.timeStamps.Count > 0)
            .SelectMany(x => x.history.timeStamps
                .Zip(x.history.data, (ts, slot) => (slot, ts))
                .SelectMany(z => z.slot.Select(p => (data: p.DeepCopy(), user: x.user.name, bucket: FloorToMinute(z.ts)))))
            .GroupBy(x => x.bucket)
            .OrderBy(g => g.Key)
            .ToList();

        var bucketKeys = allEntries.Select(g => g.Key).ToList();

        for (int i = 0; i < allEntries.Count; i++)
        {
            var topPrograms = allEntries[i]
                .GroupBy(x => x.user)
                .SelectMany(userGroup => userGroup
                    .GroupBy(x => $"{x.data.ProcessName} ({x.user})")
                    .OrderByDescending(g => g.Average(x => GetActiveResourceValue(x.data)))
                    .Take(TopCount)) 
                .OrderByDescending(g => g.Average(x => GetActiveResourceValue(x.data)))
                .ToList();

            double cumulative = 0;
            var segmentData = new List<(string name, string programName, double yBase, double yTop)>();

            foreach (var group in topPrograms)
            {
                double value = group.Average(x => GetActiveResourceValue(x.data));
                if (!_processColors.ContainsKey(group.Key))
                    _processColors[group.Key] = _palette.GetColor(_colorIndex++);

                CanvasPlot.Plot.Add.Bar(new ScottPlot.Bar
                {
                    Position = bucketKeys[i].ToOADate(),
                    Value = cumulative + value,
                    ValueBase = cumulative,
                    FillColor = _processColors[group.Key],
                    LineColor = ScottPlot.Colors.Transparent,
                    Size = TimeSpan.FromMinutes(0.8).TotalDays // fixed width in OA date units
                });

                segmentData.Add((group.Key,group.FirstOrDefault().data.ProcessName, cumulative, cumulative + value));
                cumulative += value;
            }

            _barTooltipData[bucketKeys[i].ToOADate()] = segmentData;
        }

        CanvasPlot.Plot.Axes.Bottom.IsVisible = false;
        CanvasPlot.Plot.Axes.AutoScaleX();
        SetLimits();
    }
    #endregion
    #region PieChart
    private void DrawPieChart() {
        LogEntries.Clear();
        _followFlag = true;
        CanvasPlot.Plot.Axes.Left.IsVisible =  false;
        CanvasPlot.Plot.Grid.IsVisible = false;

        var pietop = GetTopProcesses();
        if (pietop.Count == 0) return;

        double[] values = pietop.Select(p => (double)GetActiveResourceValue(p)).ToArray();

        var pie = CanvasPlot.Plot.Add.Pie(values);

        for (int i = 0; i < pietop.Count; i++)
        {
            pie.Slices[i].FillColor = _palette.GetColor(i);
            pie.Slices[i].Label = "";
            LogEntries.Add(new GraphLedgendEntry() { Title = $"{pietop[i].ProcessName} ({GetActiveResourceValue(pietop[i]):0.0} {ActiveUnit()})", 
                Color = new SolidColorBrush(
                    new Avalonia.Media.Color
                    (pie.Slices[i].FillColor.Alpha, 
                    pie.Slices[i].FillColor.R, 
                    pie.Slices[i].FillColor.G, 
                    pie.Slices[i].FillColor.B)) 
            });
        }

        pie.LineColor = ScottPlot.Colors.White;
        pie.LineWidth = 2;
        pie.DonutFraction = 0.25;

        CanvasPlot.Plot.Axes.AutoScale();
        float offset = -0.65f;
        CanvasPlot.Plot.Axes.SetLimits(-1.5 + offset, 1.5 + offset, -1.5, 1.5);
    }
    #endregion
    private float GetActiveResourceValue(IProgramData p) => ChartService.Instance.SelectedResource switch
    {
        ResourceTypes.CPU => p.CpuUsage,
        ResourceTypes.RAM => p.MemoryUsage,
        ResourceTypes.Disk => p.DiskUsage,
        ResourceTypes.Network => p.NetworkUsage,
        _ => p.CpuUsage
    };

    private List<IProgramData> GetTopProcesses()
    {
        List<IProgramData> output = new();
        var users = FolderViewData.SelectedUsersWithIndex().ToArray(); FolderViewData.SelectedUsersWithIndex().ToArray();
        List<List<IProgramData>> history;


        foreach (var user in users)
        {
            if (LiveViewModel.IsLive)
                history = ResourceService.Instance.GetProgramUsageForUser(user.name);
            else
                history = ResourceService.Instance.GetDatedProgramUsageForUser(user.name).data;

            if (history.Count == 0) continue;
            IEnumerable<IProgramData> topValues;

            if(LiveViewModel.IsLive)
                topValues = history.Last().OrderByDescending(x => GetActiveResourceValue(x)).Take(TopCount);//Top N from latest probe
            else
            {
                //Get top N averaged over the entire timeframe to get a more accurate representation of usage.
                topValues = history
                    .SelectMany(slot => slot)
                    .GroupBy(x => x.ProcessName)
                    .Select(g =>
                    {
                        var copy = g.First().DeepCopy();
                        copy.CpuUsage = (float)g.Average(x => x.CpuUsage);
                        copy.MemoryUsage = (float)g.Average(x => x.MemoryUsage);
                        copy.DiskUsage = (float)g.Average(x => x.DiskUsage);
                        copy.NetworkUsage = (float)g.Average(x => x.NetworkUsage);
                        return copy;
                    })
                    .OrderByDescending(x => GetActiveResourceValue(x))
                    .Take(TopCount);
            }
            output.AddRange(topValues);
        }
        return output;
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (ChartService.Instance.ChartType != ChartService.Bar || CanvasPlot is null) return;
        if (_barTooltipData.Count == 0) return;

        var pos = e.GetPosition(CanvasPlot);
        var dataCoords = CanvasPlot.Plot.GetCoordinates((float)pos.X, (float)pos.Y);

        List<(string name, string processname, double yBase, double yTop)>? segments = null;

        if (!LiveViewModel.IsLive)
        {
            var nearest = _barTooltipData.Keys
                .OrderBy(k => Math.Abs(k - dataCoords.X))
                .FirstOrDefault();

            if (nearest == 0 || Math.Abs(nearest - dataCoords.X) > TimeSpan.FromMinutes(0.5).TotalDays)
            {
                RemoveTooltip();
                CanvasPlot.Refresh();
                return;
            }
            segments = _barTooltipData[nearest];
        }
        else
        {
            int barIndex = (int)Math.Round(dataCoords.X);
            if (!_barTooltipData.TryGetValue(barIndex, out segments))
            {
                RemoveTooltip();
                CanvasPlot.Refresh();
                return;
            }
        }

        var hovered = segments.FirstOrDefault(s => dataCoords.Y >= s.yBase && dataCoords.Y <= s.yTop);
        if (hovered == default)
        {
            RemoveTooltip();
            CanvasPlot.Refresh();
            return;
        }

        double val = hovered.yTop - hovered.yBase;
        string text = $"{hovered.name}\n{val:0.0} {ActiveUnit()}";
        _ = LoadIconAsync(hovered.processname);
        RemoveTooltip();
        KeyTopLeftName.Content = text;
        KeyTopLeft.IsVisible = true;
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
        KeyTopLeft.IsVisible = false;
    }

    private void SetLimits()
    {
        if (ChartService.Instance.SelectedResource == ResourceTypes.RAM)
        {
            CanvasPlot.Plot.Axes.SetLimitsY(0, SystemHistory.Instance.GetTotalRam());
        }
        else if (ChartService.Instance.SelectedResource == ResourceTypes.CPU)
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
        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return;
        mainWindow.FindControl<FolderView>("FolderView")?.SetDateRange(HistoricalStartGraph, HistoricalEndGraph);
    }
    private void SetCombinationLock(bool isLocked)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if(mainWindow is null) return;
        var folderview = mainWindow.FindControl<FolderView>("FolderView");
        if (folderview is null) return;
        folderview.CombinationBox.IsLocked = isLocked;
    }
    private void SetGrid()
    {
        CanvasPlot.Plot.Grid.IsVisible = true;
        CanvasPlot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = true;
        CanvasPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = true;
        CanvasPlot.Plot.Axes.Bottom.MajorTickStyle.Length = 2;
        CanvasPlot.Plot.Axes.Left.MajorTickStyle.Length = 2;
    }
    private async Task LoadIconAsync(string appName)
    {
        Bitmap? bitmap = await Task.Run(() =>
        {
            using var db = new DBInteract();
            bool useDefaultIcon = false;
            MemoryStream? ms;
            if (db.HasIcon(appName))
            {
                CachedIcon? data = db.GetIconData(appName);
                if (data is null) return null;
                useDefaultIcon = data.IconFileType == IconFileType.None;//Has no icon.
                ms = new(data.IconData);
            }
            else//Get live icon if possible...
                ms = SystemHistory.Instance.GetIcon(appName).image;

            if (useDefaultIcon)
            {
                return TableViewModel.UnknownIcon;
            }
            else
            {
                if (ms is null) return null;

                ms.Position = 0;
                var bitmap = new Bitmap(ms);
                ms.Dispose();
                return bitmap;
            }
        });

        if (bitmap is null) return;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            KeyTopLeftPicture.Source = bitmap;
        });
    }
}
