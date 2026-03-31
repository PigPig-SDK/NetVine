using System;
namespace UI.ViewModels;

public class ChartService
{
    public static readonly ChartService Instance = new();

    public const string CPUChartname = "CPU";
    public const string RAMChartName = "RAM";
    public const string DiskChartName = "DISK";
    public const string NetworkChartName = "NET";

    private string _chartType = "Line";
    public string ChartType
    {
        get => _chartType;
        set
        {
            _chartType = value;
            ChartTypeChanged?.Invoke();
        }
    }

    private string _selectedResource = CPUChartname;
    public string SelectedResource
    {
        get => _selectedResource;
        set { _selectedResource = value; ChartTypeChanged?.Invoke(); }
    }
    public event Action? ChartTypeChanged;
}