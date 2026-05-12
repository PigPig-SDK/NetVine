using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Threading.Tasks;
using UI.ViewModels;

namespace UI.Views;

public class ToastWindow : Window
{
    private const double _margin = 30;
    private const double _toastWidth = 300;
    private const double _toastHeight = 60;

    private Control locationControl;
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
            (int)(workingArea.X + workingArea.Width - _toastWidth - _margin),
            (int)(workingArea.Y + workingArea.Height - _toastHeight - _margin)
        );
        Background = new SolidColorBrush(Color.Parse("#00000000"));//Transparent.

        MainWindowViewModel.OnAnimateFrame += Animate;

        
        TextBlock tb = new TextBlock
        {
            Text = message,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(16, 0),
            TextWrapping = TextWrapping.Wrap
        };
        locationControl = tb;

        Content = locationControl;
    }

    private void Animate(double time)
    {
        locationControl.Margin = new Thickness(MathF.Sin((float)time * 10) * 10);
    }

    public async Task ShowToast(int durationMs = 3000)
    {
        Show();
        await Task.Delay(durationMs);
        MainWindowViewModel.OnAnimateFrame -= Animate;
        Close();
    }
}