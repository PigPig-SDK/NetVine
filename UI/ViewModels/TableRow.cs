using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Core;
using Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public class TableRow : ObservableObject
    {
        // Existing properties
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
            set
            {
                _historicalData = value;
                OnPropertyChanged(nameof(HistoricalData));
            }
        }

        
        
        
        // Icon properties
        private Bitmap? _icon;
        private bool _iconLoaded = false;

        //methods

        /// <summary>
        /// appends to a debug file, with all data from the row.
        /// </summary>
        /// <param name="path"></param>
        public void PrintToFile(string path = "tablerow_debug.txt")
        {
            var fullPath = path;
            var live = LiveData == null ? "null" : $"AppName={AppName} SystemName = {SystemName} CPU={LiveData.CpuUsage} MEM={LiveData.MemoryUsage}";
            var hist = HistoricalData == null ? "null" : $"AppName={AppName} SystemName={SystemName} CpuAvg={HistoricalData.CpuUsageAvg} CpuPeak={HistoricalData.CpuUsagePeak} MemAvg={HistoricalData.MemoryUsageAvg}";

            var line = $"{SystemName} | {AppName} | {live} | {hist}";
            System.IO.File.AppendAllText(fullPath, line + Environment.NewLine);
        }

        public Bitmap? AppIcon
        {
            get
            {
                if (!_iconLoaded)
                {
                    _iconLoaded = true;
                    _ = LoadIconAsync();
                }
                return _icon;
            }
        }

        private async Task LoadIconAsync()
        {
            var bitmap = await Task.Run(() =>
            {
                try
                {
                    var process = System.Diagnostics.Process
                        .GetProcessesByName(AppName)
                        .FirstOrDefault();

                    if (process?.MainModule?.FileName is not { } path)
                        return null;

                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                    if (icon == null) return null;

                    using var bmp = icon.ToBitmap();
                    using var ms = new System.IO.MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    return new Bitmap(ms);
                }
                catch { return null; }
            });

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _icon = bitmap;
                OnPropertyChanged(nameof(AppIcon));
            });
        }
    }
}