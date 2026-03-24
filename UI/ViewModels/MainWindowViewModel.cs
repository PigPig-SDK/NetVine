using System;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const int GraphView = 0;//from merge conflict
    public const int TableView = 1;//

    public static Action<int>? OnTabChanged;

    public string Greeting { get; } = "Your moms car.";
    public static int ActiveTab { get; set; }

    public static Action<Avalonia.Input.TextInputEventArgs, string?>? OnSearchSubmission;

    public static Action<string?>? OnSearchKeyStroke;
}
