using System;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const int GraphView = 0;
    public const int TableView = 1;
    public string Greeting { get; } = "Welcome to Avalonia!";

    public static Action<int>? OnTabChanged;

}
