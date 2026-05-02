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
        public List<double> CpuHistory => _resourceService.CpuHistory;
        public List<double> CpuAvgHistory => _resourceService.CpuAvgHistory;
        public List<double> CpuPeakHistory => _resourceService.CpuPeakHistory;
        
        public List<double> RamHistory => _resourceService.RamHistory;
        public List<double> RamAvgHistory => _resourceService.RamAvgHistory;
        public List<double> RamPeakHistory => _resourceService.RamPeakHistory;
        
        public List<double> DiskHistory => _resourceService.DiskHistory;
        public List<double> DiskAvgHistory => _resourceService.DiskAvgHistory;
        public List<double> DiskPeakHistory => _resourceService.DiskPeakHistory;
        
        public List<double> NetworkHistory => _resourceService.NetworkHistory;
        public List<double> NetworkAvgHistory => _resourceService.NetworkAvgHistory;
        public List<double> NetworkPeakHistory => _resourceService.NetworkPeakHistory;

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
