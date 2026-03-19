using CommunityToolkit.Mvvm.ComponentModel;
using Core;
using Infrastructure;


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

        private ProgramDataHistorical? _historicalData;
        public ProgramDataHistorical? HistoricalData
        {
            get => _historicalData;
            set { _historicalData = value; OnPropertyChanged(); }
        }
    }
}
