using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UI.ViewModels.CanvasViewModel;

namespace UI.ViewModels
{
    public class CanvasViewModel : ViewModelBase
    {
        private readonly ChartService _chartService;
        private readonly ResourceService _resourceService;

        public double RAMTotal => _resourceService.RAMTotal;

        public event Action? ChartUpdateRequested;
        public string CurrentChartType => _chartService.ChartType;

        public List<IProgramData> LatestSnapshot => _resourceService.LatestSnapshot;

        public List<List<IProgramData>> SnapshotHistory => _resourceService.SnapshotHistory;
        
        public List<List<IProgramData>> AllHistorical => _resourceService.AllHistorical;
        public List<double> CpuHistory => _resourceService.LiveCpuHistory;
        public List<double> CpuAvgHistory => _resourceService.HistoricalCpuHistory;
        public List<double> RamHistory => _resourceService.LiveRamHistory;
        public List<double> RamAvgHistory => _resourceService.HistoricalRamHistory;
        
        public List<double> DiskHistory => _resourceService.LiveDiskHistory;
        public List<double> DiskAvgHistory => _resourceService.HistoricalDiskHistory;
        
        public List<double> NetworkHistory => _resourceService.LiveNetworkHistory;
        public List<double> NetworkAvgHistory => _resourceService.HistoricalNetworkHistory;

        public string SelectedResource => _chartService.SelectedResource;

        public CanvasViewModel(ChartService chartService, ResourceService resourceService)
        {
            _chartService = chartService;
            _chartService.ChartTypeChanged += UpdateChart;
            _resourceService = resourceService;
            _resourceService.DataUpdated += UpdateChart;
        }

        private void UpdateChart()
        {
            ChartUpdateRequested?.Invoke();
        }
    }
}
