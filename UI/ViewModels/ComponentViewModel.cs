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
        
        public ICommand SelectCpuAvgCommand { get; }
        
        public ICommand SelectCpuPeakCommand { get; }
        public ICommand SelectRamCommand { get; }
        
        public ICommand SelectRamAvgCommand { get; }
        
        public ICommand SelectRamPeakCommand { get; }
        public ICommand SelectDiskCommand { get; }
        
        public ICommand SelectDiskAvgCommand { get; }
        
        public ICommand SelectDiskPeakCommand { get; }
        
        public ICommand SelectNetworkCommand { get; }
        
        public ICommand SelectNetworkAvgCommand { get; }
        
        public ICommand SelectNetworkPeakCommand { get; }
        

        public bool ComponentIsLive => LiveViewModel.IsLive;
        public bool ComponentIsNotLive => !LiveViewModel.IsLive;

        public ComponentViewModel(ChartService chartService, ResourceService resourceService)
        {
            _chartService = chartService;
            _resourceService = resourceService;
            _resourceService.DataUpdated -= OnDataUpdated;
            _resourceService.DataUpdated += OnDataUpdated;
            LiveViewModel.ViewChangedEvent -= ViewChangedLive;
            LiveViewModel.ViewChangedEvent += ViewChangedLive;

            SelectCpuCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.CPU);
            SelectCpuAvgCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.CPUAvg);
            SelectCpuPeakCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.CPUPeak);
            
            SelectRamCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.RAM);
            SelectRamAvgCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.RAMAvg);
            SelectRamPeakCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.RAMPeak);
            
            SelectDiskCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.DISK);
            SelectDiskAvgCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.DISKAvg);
            SelectDiskPeakCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.DISKPeak);
            
            SelectNetworkCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.NET);
            SelectNetworkAvgCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.NETAvg);
            SelectNetworkPeakCommand = new RelayCommand(() => _chartService.SelectedResource = ChartService.NETPeak);
            
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
        public string ButtonCPUAvgText => $"{_resourceService.CpuUsageAvg:0.0}%";
        public string ButtonCPUPeakText => $"{_resourceService.CpuUsagePeak:0.0}%";
        
        public string ButtonRAMText => $"{_resourceService.RamUsage:0.0} MB";
        public string ButtonRAMAvgText => $"{_resourceService.RamUsageAvg:0.0} MB";
        public string ButtonRAMPeakText => $"{_resourceService.RamUsagePeak:0.0} MB";
        
        public string ButtonDISKText => $"{_resourceService.DiskUsage:0.0} MB/s";
        public string ButtonDISKAvgText => $"{_resourceService.DiskUsageAvg:0.0} MB/s";
        public string ButtonDISKPeakText => $"{_resourceService.DiskUsagePeak:0.0} MB/s";
        
        public string ButtonNetworkText => $"{_resourceService.NetworkUsage:0.0} MB/s";
        public string ButtonNetworkAvgText => $"{_resourceService.NetworkUsageAvg:0.0} MB/s";
        public string ButtonNetworkPeakText => $"{_resourceService.NetworkUsagePeak:0.0} MB/s";

        private void OnDataUpdated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUAvgText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUPeakText)));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMAvgText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMPeakText)));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKAvgText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKPeakText)));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkAvgText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkPeakText)));
        }

        private void ViewChangedLive(bool isLive)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComponentIsLive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComponentIsNotLive)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

