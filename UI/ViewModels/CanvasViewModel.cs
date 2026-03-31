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

        public event Action? ChartUpdateRequested;
        public string CurrentChartType => _chartService.ChartType;

        public List<double> CpuHistory => _resourceService.CpuHistory;
        public List<double> RamHistory => _resourceService.RamHistory;
        public List<double> DiskHistory => _resourceService.DiskHistory;
        public List<double> NetworkHistory => _resourceService.NetworkHistory;

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
