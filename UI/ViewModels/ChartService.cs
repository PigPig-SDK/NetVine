using System;
namespace UI.ViewModels;

using Core;
using Infrastructure.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

public class ChartService
{
    public static readonly ChartService Instance = new();

    public event Action? ChartTypeChanged;

    public const string Line = "Line";
    public const string Bar = "Bar";
    public const string Pie = "Pie";

    private ResourceTypes _selectedResource = ResourceTypes.CPU;
    public ResourceTypes SelectedResource
    {
        get => _selectedResource;
        set { _selectedResource = value; ChartTypeChanged?.Invoke(); }
    }

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
}