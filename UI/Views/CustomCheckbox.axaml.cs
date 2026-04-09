using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using YamlDotNet.Core.Tokens;

namespace UI;

public partial class CustomCheckbox : UserControl
{

    public event EventHandler<bool>? CheckedChanged;

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

    public static readonly StyledProperty<string> LockedSourceProperty =
        AvaloniaProperty.Register<CustomCheckbox, string>(nameof(LockedSource), defaultValue: "avares://NetVine/Assets/lock.png");

    public string LockedSource
    {
        get => GetValue(LockedSourceProperty);
        set
        {
            SetValue(LockedSourceProperty, value);
            LockedIcon.Source = new Bitmap(AssetLoader.Open(new Uri(LockedSource)));
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

    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<CustomCheckbox, bool>(nameof(IsLocked), defaultValue: false);
    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set
        {
            SetValue(IsLockedProperty, value);
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
        SelectorButton.IsHitTestVisible = !IsLocked;
        SelectorButton.IsEnabled = !IsLocked;

        SelectorButton.IsChecked = IsChecked;
        LockedIcon.IsVisible = IsLocked;
        if (IsLocked)
        {
            UncheckedIcon.IsVisible = false;
            CheckedIcon.IsVisible = false;
        }
        else
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
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
        CheckedChanged?.Invoke(this, IsChecked);
    }
}