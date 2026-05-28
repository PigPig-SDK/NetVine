using CommunityToolkit.Mvvm.Input;
using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.ViewModels
{
    public class ComponentViewModel : INotifyPropertyChanged
    {
        public ICommand SelectCpuCommand { get; }
        public ICommand SelectRamCommand { get; }
        public ICommand SelectDiskCommand { get; }
        public ICommand SelectNetworkCommand { get; }
        
        public bool ComponentIsLive => LiveViewModel.IsLive;
        public bool ComponentIsNotLive => !LiveViewModel.IsLive;

        public ComponentViewModel()
        {
            ResourceService.Instance.DataUpdated -= OnDataUpdated;
            ResourceService.Instance.DataUpdated += OnDataUpdated;
            LiveViewModel.ViewChangedEvent -= ViewChangedLive;
            LiveViewModel.ViewChangedEvent += ViewChangedLive;

            SelectCpuCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.CPU);
            SelectRamCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.RAM);
            SelectDiskCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.Disk);
            SelectNetworkCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.Network);
            
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
            }
        }
        public string ButtonCPUText => $"{ResourceService.Instance.CpuUsage:0.0}%";
        public string ButtonRAMText => $"{ResourceService.Instance.RamUsage:0.0} MB";
        public string ButtonDISKText => $"{ResourceService.Instance.DiskUsage:0.0} MB/s";
        public string ButtonNetworkText => $"{ResourceService.Instance.NetworkUsage:0.0} MB/s";
        private void OnDataUpdated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkText)));
        }

        private void ViewChangedLive(bool isLive)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComponentIsLive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComponentIsNotLive)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

