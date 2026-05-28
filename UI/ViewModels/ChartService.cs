using System;
namespace UI.ViewModels;
using Infrastructure.Networking;
using System.Linq;

public class ChartService
{
    public static readonly ChartService Instance = new();

    public const string CPU = "CPU";
    public const string CPUHistory = "CPU History";
    
    public const string RAM = "RAM";
    public const string RAMHistory = "RAM History";
    
    public const string DISK = "DISK";
    public const string DISKHistory = "DISK History";
    
    public const string NET = "NET";
    public const string NETHistory = "NET History";
    

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
    private string? _selectedDevice = null;
    public string? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            ChartTypeChanged?.Invoke();
        }
    }
    private ChartService()
    {
        FolderViewData.OnSelectionUpdated += OnSelectionUpdated;
    }
    private void OnSelectionUpdated()
    {
        var selected = FolderViewData.UserMapping.Values
            .FirstOrDefault(u => u.IsSelected && u.IsOnline);

        SelectedDevice = selected?.Username ?? Environment.MachineName;
    }

    public  string LiveSwap(string chartMode)
    {
        switch (chartMode)
        {
            case CPU:
                return CPUHistory;
            case RAM:
                return RAMHistory;
            case DISK:
                return DISKHistory;
            case NET:
                return NETHistory;
            
            case CPUHistory:
                return CPU;
            case RAMHistory:
                return RAM;
            case DISKHistory:
                return DISK;
            case NETHistory:
                return NET;
        }
        return null;
    }
    public event Action? ChartTypeChanged;
}