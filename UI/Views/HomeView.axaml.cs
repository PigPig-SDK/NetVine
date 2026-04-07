using Avalonia;
using Avalonia.Controls;
using System;

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
}