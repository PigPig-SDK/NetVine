using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace UI;

public partial class LoadingComponent : UserControl
{
    public LoadingComponent()
    {
        InitializeComponent();
    }
    public void Animate(double time)
    {
        LoadingCircle.StrokeDashOffset = (time * 8) + MathF.Sin((float)time * 2) * 10;

    }
}