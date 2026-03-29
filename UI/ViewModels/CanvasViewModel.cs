using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UI.ViewModels.CanvasViewModel;

namespace UI.ViewModels
{
    public class CanvasViewModel : ViewModelBase
    {
        private readonly ChartService _chartService;

        public event Action? ChartUpdateRequested;
        public string CurrentChartType => _chartService.ChartType;

        public CanvasViewModel(ChartService chartService)
        {
            _chartService = chartService;
            _chartService.ChartTypeChanged += UpdateChart;
        }

        private void UpdateChart()
        {
        }
    }
}
