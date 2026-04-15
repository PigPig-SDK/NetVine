using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace UI.Views
{
    public partial class TimeFrameSelectionWindow : Window
    {
        private static  DateTime? HistoricalStartInternal { get; set; }
        
        private static DateTime? HistoricalEndInternal { get; set; }
        public TimeFrameSelectionWindow()
        {
            InitializeComponent();

            if (!HistoricalStartInternal.HasValue && !HistoricalEndInternal.HasValue)
            {
                Date1Selector.SelectedDate = DateTime.Now.Date;
                Date2Selector.SelectedDate = DateTime.Now.Date;
                Time1Selector.SelectedTime = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMinutes(10));
                Time2Selector.SelectedTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(10));    
            }
            else
            {
                Date1Selector.SelectedDate = HistoricalStartInternal?.Date;
                Date2Selector.SelectedDate = HistoricalEndInternal?.Date;
                Time1Selector.SelectedTime = HistoricalStartInternal?.TimeOfDay;
                Time2Selector.SelectedTime = HistoricalEndInternal?.TimeOfDay;
                
            }
            
        }

        private void Select(object? sender, RoutedEventArgs e)
        {
           
            DateTime? selectedStart = Date1Selector.SelectedDate?.Date + Time1Selector.SelectedTime;
            DateTime? selectedEnd = Date2Selector.SelectedDate?.Date + Time2Selector.SelectedTime;
            
            HistoricalStartInternal = selectedStart;
            HistoricalEndInternal = selectedEnd;
            
            Close((selectedStart, selectedEnd));
        }

        private void EmptyDate1(object? sender, RoutedEventArgs e)
        {
            Date1Selector.SelectedDate = null;
            Time1Selector.SelectedTime = null;
        }
        private void EmptyDate2(object? sender, RoutedEventArgs e)
        {
            Date2Selector.SelectedDate = null;
            Time2Selector.SelectedTime = null;
        }

        private void Exit(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}