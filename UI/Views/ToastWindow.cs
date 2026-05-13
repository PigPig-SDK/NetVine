using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Core;
using Infrastructure;
using Infrastructure.Notifications;
using System;
using System.Threading.Tasks;
using UI.ViewModels;

namespace UI.Views;

public class ToastWindow : Window
{
    private const double _toastWidth = 300;
    private const double _toastHeight = 60;
    private double _durationMs = 0;

    private DispatcherTimer? _animationTimer;

    private ToastBody? locationControl;
    private int animationRefire = 16;
    DateTime startTime = DateTime.Now;

    public ToastWindow(Notification notification)
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

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(animationRefire)//60fps
        };
        _animationTimer.Tick += Animate;

        ToastBody tb = new ToastBody
        {
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(16, 0),
            Width = _toastWidth-20,
            Height = _toastHeight,
            Title = notification.Title,
        };
        tb.SetPinColor(ComputeColor(notification));
        locationControl = tb;
        Avalonia.Controls.Canvas.SetBottom(locationControl, -1000);//Start underground.
        Avalonia.Controls.Canvas.SetLeft(locationControl, 0);

        Avalonia.Controls.Canvas? canvas = new() { Children = { tb } };
        Content = canvas;
    }

    private void Animate(object? sender, EventArgs e)
    {
        var delta = DateTime.Now - startTime;

        AnimateLocal(delta.TotalSeconds);
    }

    private Color ComputeColor(Notification notification)
    {
        switch(notification.Priority)
        {
            case NotificationPriority.Alert:
                return new Color(255, 255, 238, 140);
            case NotificationPriority.Critical:
                return new Color(255, 255, 116, 108);
            case NotificationPriority.Message:
                return new Color(255, 255, 255, 255);
        }

        return new Color(255, 255, 255, 255);
    }

    private void AnimateLocal(double time)
    {
        if (locationControl is null) return;

        double startY = -100;
        double endY = 0;
        //Retreat message...
        if (time >= (_durationMs/1000.0f) - 1)
        {
            startY = 0;
            endY = -100;
            time -= (_durationMs / 1000.0f) - 1;
        }
        
        double t = Math.Clamp(time, 0, 1);
        double eased = t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);

        double current = startY + (endY - startY) * eased;
        Avalonia.Controls.Canvas.SetBottom(locationControl, current);
        locationControl.Animate(time);
    }

    public async Task ShowToast(int durationMs = 3000)
    {
        _durationMs = durationMs;
        startTime = DateTime.Now;
        if (ConfigManager.ReadSettingBool(SettingInt.DisableToastPopups)) return;
        if(_animationTimer is not null)
            _animationTimer.Start();
        Show();
        await Task.Delay(durationMs);

        if (_animationTimer is not null)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= Animate;
        }
        Close();
    }
}