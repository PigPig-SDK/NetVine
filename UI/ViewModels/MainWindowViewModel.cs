using System;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Your moms car.";

    public static Action<Avalonia.Input.TextInputEventArgs>? OnSearchSubmission;

    public static Action<Avalonia.Input.KeyEventArgs>? OnSearchKeyStroke;
}
