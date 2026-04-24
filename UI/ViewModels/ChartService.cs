using System;
namespace UI.ViewModels;

public class ChartService
{
    public static readonly ChartService Instance = new();

    public const string CPU = "CPU";
    public const string CPUAvg = "CPU Avg";
    public const string CPUPeak = "CPU Peak";
    public const string RAM = "RAM";
    public const string RAMAvg = "RAM Avg";
    public const string RAMPeak = "RAM Peak";
    public const string DISK = "DISK";
    public const string DISKAvg = "DISK Avg";
    public const string DISKPeak = "DISK Peak";
    public const string NET = "NET";
    public const string NETAvg = "NET Avg";
    public const string NETPeak = "NET Peak";
    public const string NETTotal = "NET Total";
    

    public const string Line = "Line";
    public const string Bar = "Bar";
    public const string Pie = "Pie";

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

    private string _selectedResource = CPU;
    public string SelectedResource
    {
        get => _selectedResource;
        set { _selectedResource = value; ChartTypeChanged?.Invoke(); }
    }
    public event Action? ChartTypeChanged;
}