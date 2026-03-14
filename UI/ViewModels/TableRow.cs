using CommunityToolkit.Mvvm.ComponentModel;
using Core;


namespace UI.ViewModels
{
    /// <summary>
    /// Represents a row of performance metrics for a specific system application, including CPU, RAM, disk, and
    /// network usage statistics. This is also sort of a placeholder class. Planning on adding some columns.
    /// </summary>
    /// <remarks>This class is designed to hold average and maximum resource usage values, which can
    /// be useful for monitoring and analyzing application performance over time.</remarks>
    public class TableRow : ObservableObject
    {
        public string SystemName { get; set; }
        public string AppName { get; set; }

        private IProgramData? _liveData;
        public IProgramData? LiveData
        {
            get => _liveData;
            set { _liveData = value; OnPropertyChanged(); }
        }

        private IProgramData? _totalData;
        public IProgramData? TotalData
        {
            get => _totalData;
            set { _totalData = value; OnPropertyChanged(); }
        }

        private float _cpuAvg;
        public float CpuAvg
        {
            get => _cpuAvg;
            set { _cpuAvg = value; OnPropertyChanged(); }
        }

        private float _cpuMax;
        public float CpuMax { get => _cpuMax; set { _cpuMax = value; OnPropertyChanged(); } }

        private float _ramAvg;
        public float RamAvg { get => _ramAvg; set { _ramAvg = value; OnPropertyChanged(); } }

        private float _ramMax;
        public float RamMax { get => _ramMax; set { _ramMax = value; OnPropertyChanged(); } }

        private float _diskAvg;
        public float DiskAvg { get => _diskAvg; set { _diskAvg = value; OnPropertyChanged(); } }

        private float _diskMax;
        public float DiskMax { get => _diskMax; set { _diskMax = value; OnPropertyChanged(); } }

        private float _networkAvg;
        public float NetworkAvg { get => _networkAvg; set { _networkAvg = value; OnPropertyChanged(); } }

        private float _networkMax;
        public float NetworkMax { get => _networkMax; set { _networkMax = value; OnPropertyChanged(); } }

    }
}
