using System;
namespace UI.ViewModels;

public class ChartService
{
    public static readonly ChartService Instance = new();
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

    private string _selectedResource = "CPU";
    public string SelectedResource
    {
        get => _selectedResource;
        set { _selectedResource = value; ChartTypeChanged?.Invoke(); }
    }
    public event Action? ChartTypeChanged;
}