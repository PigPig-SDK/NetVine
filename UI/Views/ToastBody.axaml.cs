using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}