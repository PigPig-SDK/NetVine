using System;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Your moms car.";
    public static int ActiveTab { get; set; }

    public static Action<Avalonia.Input.TextInputEventArgs, string?>? OnSearchSubmission;

    public static Action<string?>? OnSearchKeyStroke;
}
