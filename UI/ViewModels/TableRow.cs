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
        public string SystemName { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;

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
            Bitmap? bitmap = await Task.Run(() =>
            {
                using var db = new DBInteract();
                bool useDefaultIcon = false;
                MemoryStream? ms;
                if (db.HasIcon(AppName))
                {
                    CachedIcon? data = db.GetIconData(AppName);
                    if (data is null) return null;
                    useDefaultIcon = data.IconFileType == IconFileType.None;//Has no icon.
                    ms = new(data.IconData);
                }
                else//Get live icon if possible...
                    ms = SystemHistory.Instance.GetIcon(AppName).image;

                if (useDefaultIcon)
                {
                    return TableViewModel.UnknownIcon;
                }
                else
                {
                    if (ms is null) return null;

                    ms.Position = 0;
                    var bitmap = new Bitmap(ms);
                    ms.Dispose();
                    return bitmap;
                }
            });

            if(bitmap is null) return;

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _icon = bitmap;
                OnPropertyChanged(nameof(AppIcon));
            });
        }
    }
}