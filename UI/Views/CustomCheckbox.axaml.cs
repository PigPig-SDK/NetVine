using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace UI;

public partial class CustomCheckbox : UserControl
{

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<CustomCheckbox, string>(nameof(Title), defaultValue: "Default");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> CheckedSourceProperty =
        AvaloniaProperty.Register<CustomCheckbox, string>(nameof(CheckedSource), defaultValue: "avares://NetVine/Assets/live_icon_ticked.png");

    public string CheckedSource
    {
        get => GetValue(CheckedSourceProperty);
        set
        {
            SetValue(CheckedSourceProperty, value);
            CheckedIcon.Source = new Bitmap(AssetLoader.Open(new Uri(CheckedSource)));
        }
    }

    public static readonly StyledProperty<string> UncheckedSourceProperty =
        AvaloniaProperty.Register<CustomCheckbox, string>(nameof(UncheckedSource), defaultValue: "avares://NetVine/Assets/live_icon.png");

    public string UncheckedSource
    {
        get => GetValue(UncheckedSourceProperty);
        set
        {
            SetValue(UncheckedSourceProperty, value);
            UncheckedIcon.Source = new Bitmap(AssetLoader.Open(new Uri(UncheckedSource)));
        }
    }

    public static new readonly StyledProperty<int> FontSizeProperty =
        AvaloniaProperty.Register<CustomCheckbox, int>(nameof(FontSize), defaultValue: 30);

    public new int FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<CustomCheckbox, bool>(nameof(IsChecked), defaultValue: false);
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set
        {
            SetValue(IsCheckedProperty, value);
            UpdateChecked();
        }
    }

    public CustomCheckbox()
    {
        InitializeComponent();
        DataContext = this;        
        UpdateChecked();
    }

    void UpdateChecked()
    {
        if (IsChecked)
        {
            CheckedIcon.IsVisible = true;
            UncheckedIcon.IsVisible = false;
        }
        else
        {
            CheckedIcon.IsVisible = false;
            UncheckedIcon.IsVisible = true;
        }
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
    }
}