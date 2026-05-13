using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace UI;

public partial class ToastBody : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
    AvaloniaProperty.Register<CustomCheckbox, string>(nameof(Title), defaultValue: "Message...");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public ToastBody()
    {
        InitializeComponent();
        DataContext = this;
    }
    private void PullOpenWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(App.Current is App app)
            app.ShowWindow(null, e);
    }
    public void Animate(double time)
    {
        if (ActivityCircle is null) return;
        ActivityCircle.StrokeDashOffset = (time * 4);
    }
    public void SetPinColor(Color color)
    {
        ActivityCircle.Fill = new SolidColorBrush(color);
    }
}