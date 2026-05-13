using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Core;
using Infrastructure;
using ScottPlot.Colormaps;
using SkiaSharp;
using System;
using System.Threading.Tasks;
using UI.ViewModels;

namespace UI.Views;

public class ToastWindow : Window
{
    private const double _toastWidth = 300;
    private const double _toastHeight = 60;
    private double? _animStartTime = null;
    private double _durationMs = 0;

    private ToastBody? locationControl;
    public ToastWindow(string message)
    {
        var screen = Screens.Primary;
        if (screen is null)
        {
            Debug.Log("Cannot show toast screen. No main screen!");
            return;
        }

        var workingArea = screen.WorkingArea;

        Title = "";
        Width = _toastWidth;
        Height = _toastHeight;
        CanResize = false;
        ShowInTaskbar = false;
        SystemDecorations = SystemDecorations.None;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Topmost = true;
        Position = new PixelPoint(
            (int)(workingArea.X + workingArea.Width - _toastWidth),
            (int)(workingArea.Y + workingArea.Height - _toastHeight)
        );
        Background = new SolidColorBrush(Color.Parse("#00000000"));//Transparent.

        MainWindowViewModel.OnAnimateFrame += Animate;

        ToastBody tb = new ToastBody
        {
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(16, 0),
            Width = _toastWidth-20,
            Height = _toastHeight,
            Title = message,
        };
        tb.SetPinColor(ComputeColor());
        locationControl = tb;
        Avalonia.Controls.Canvas.SetBottom(locationControl, -1000);//Start underground.
        Avalonia.Controls.Canvas.SetLeft(locationControl, 0);

        Avalonia.Controls.Canvas? canvas = new() { Children = { tb } };

        Content = canvas;
    }

    private Color ComputeColor()
    {
        return new Color(255, 255, 255, 255);
    }

    private void Animate(double time)
    {
        if(_animStartTime is null)
        {
            _animStartTime = time;
            return;
        }

        if (locationControl is null) return;

        double localTime = time - _animStartTime.Value;

        double startY = -100;
        double endY = 0;
        //Retreat message...
        if (localTime >= (_durationMs/1000.0f) - 1)
        {
            startY = 0;
            endY = -100;
            localTime -= 2;
        }

        
        double t = Math.Clamp(localTime, 0, 1);
        double eased = t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);

        double current = startY + (endY - startY) * eased;
        Avalonia.Controls.Canvas.SetBottom(locationControl, current);
        locationControl.Animate(time);
    }

    public async Task ShowToast(int durationMs = 3000)
    {
        _durationMs = durationMs;
        if (ConfigManager.ReadSettingBool(SettingInt.DisableToastPopups)) return;

        Show();
        await Task.Delay(durationMs);
        MainWindowViewModel.OnAnimateFrame -= Animate;
        Close();
    }
}