using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace UI.Views
{
    public partial class TimeFrameSelectionWindow : Window
    {
        public TimeFrameSelectionWindow()
        {
            InitializeComponent();
            
            //Date1Selector.SelectedDate = DateTime.Now.Date;
            //Date2Selector.SelectedDate = DateTime.Now.Date;
            
            //Time1Selector.SelectedTime = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMinutes(10));
            //Time2Selector.SelectedTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(10));;
            
        }

        private void Select(object? sender, RoutedEventArgs e)
        {
           
            DateTime? selectedStart = Date1Selector.SelectedDate?.Date + Time1Selector.SelectedTime;
            DateTime? selectedEnd = Date1Selector.SelectedDate?.Date + Time1Selector.SelectedTime;
            
            Close((selectedStart, selectedEnd));
        }

        private void Exit(object? sender, RoutedEventArgs e)
        {
            (DateTime? start, DateTime? end) noTimeSelected = (null, null);
            
            Close(noTimeSelected);
        }
    }
}