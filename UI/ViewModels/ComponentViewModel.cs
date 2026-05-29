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
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand SelectCpuCommand { get; }
        public ICommand SelectRamCommand { get; }
        public ICommand SelectDiskCommand { get; }
        public ICommand SelectNetworkCommand { get; }

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
        public ComponentViewModel()
        {
            ResourceService.Instance.DataUpdated += OnDataUpdated;

            SelectCpuCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.CPU);
            SelectRamCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.RAM);
            SelectDiskCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.Disk);
            SelectNetworkCommand = new RelayCommand(() => ChartService.Instance.SelectedResource = ResourceTypes.Network);

        }
        private void OnDataUpdated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonNetworkText)));
        }

    }
}

