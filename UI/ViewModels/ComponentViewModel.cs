using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.ViewModels
{
    public class ComponentViewModel : INotifyPropertyChanged
    {
        private readonly ChartService _chartService;
        private readonly ResourceService _resourceService;

        public ICommand SelectCpuCommand { get; }
        public ICommand SelectRamCommand { get; }
        public ICommand SelectDiskCommand { get; }
        public ICommand SelectNetworkCommand { get; }

        public ComponentViewModel(ChartService chartService, ResourceService resourceService)
        {
            _chartService = chartService;
            _resourceService = resourceService;
            _resourceService.DataUpdated += OnDataUpdated;

            SelectCpuCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.CPU);
            SelectRamCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.RAM);
            SelectDiskCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.DISK);
            SelectNetworkCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.NET);
        }

        public List<string> ChartTypes { get; } = new() { "Line", "Bar", "Pie" };

        private string _selectedChartType = "Line";
        public string SelectedChartType
        {
            get => _selectedChartType;
            set
            {
                _selectedChartType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChartType)));
                _chartService.ChartType = value;
            }
        }
        public string ButtonCPUText => $"{_resourceService.CpuUsage:0.0}%";
        public string ButtonRAMText => $"{_resourceService.RamUsage:0.0} MB";
        public string ButtonDISKText => $"{_resourceService.DiskUsage:0.0}%";
        public string ButtonNetworkText => $"{_resourceService.NetworkUsage:0.0} MB/s";

        private void OnDataUpdated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkText)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

