using System;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const int GraphView = 0;//from merge conflict
    public const int TableView = 1;//

    public static event Action<int>? OnTabChanged;

    // I guess events can't be invoked from outside the class,
    // this is just a workaround for that
    public static void RaiseTabChanged(int tabIndex)
    {
        OnTabChanged?.Invoke(tabIndex);
    }

    public string Greeting { get; } = "Your moms car.";
    public static int ActiveTab { get; set; }

    public static Action<Avalonia.Input.TextInputEventArgs, string?>? OnSearchSubmission;

    public static Action<string?>? OnSearchKeyStroke;
}
