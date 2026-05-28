using Avalonia;
using Avalonia.Controls;
using Infrastructure;
using Infrastructure.Notifications;
using System;
using System.Diagnostics;
using System.IO;

namespace UI;

public partial class HomeView : UserControl
{
    public static string[] Slogans =
        ["Always tracking.",
        "Open for buisness",
        "The only ____ you'll ever need.",
        "3D glasses not included.",
        "Selling your data...",
        "A family friend",
        "Submitting your piracy to the authorities",
        "From your least favorite armchair critics",
        "Wait. You can sell your soul?"];

    public HomeView()
    {
        InitializeComponent();
        GenerateSlogan();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        GenerateSlogan();
    }
    void GenerateSlogan()
    {
        Slogan.Text = Slogans[new Random().Next(Slogans.Length)];
    }

    private void OpenConfigLocation(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine"));
        }
        else
        {
            NotificationManager.WriteNotification(new Notification(NotificationPriority.Critical, "OS Error!", "Cannot open configuration automatically on your OS"));
        }
    }
}