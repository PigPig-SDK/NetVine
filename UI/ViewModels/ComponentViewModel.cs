using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public class ComponentViewModel : INotifyPropertyChanged
    {
        private readonly ChartService _chartService;

        public ComponentViewModel(ChartService chartService)
        {
            _chartService = chartService;
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



        //whatever value the graph last received, update these with that value
        private string CPUtext = "0.0%";
        private string GPUtext = "0.0%";
        private string RAMtext = "0.0%";
        private string DISKtext = "0.0%";

        public string ButtonCPUText
        {
            get => CPUtext;
            set
            {
                CPUtext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonCPUText)));
            }
        }
        public string ButtonGPUText
        {
            get => GPUtext;
            set
            {
                GPUtext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonGPUText)));
            }
        }
        public string ButtonRAMText
        {
            get => RAMtext;
            set
            {
                RAMtext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonRAMText)));
            }
        }
        public string ButtonDISKText
        {
            get => DISKtext;
            set
            {
                DISKtext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonDISKText)));
            }
        }


        public event Action? ChartTypeChanged; 
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
