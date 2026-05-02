using Avalonia.Threading;
using System;
using System.Threading;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const int GraphView = 0;
    public const int TableView = 1;
    public const int SettingView = 2;
    public const int HomeView = 3;

    public static event Action<int>? OnTabChanged;

    private static Lock _timerLock = new Lock();
    private static System.Timers.Timer? _animationTimer;
    private static double _animationTime = 0;
    private static double _animationRefire = 0.015;
    /// <summary>
    /// CAREFUL! THIS IS A STATIC EVENT.
    /// FAILURE TO UNBIND FROM THIS EVENT WILL CAUSE A MEMORY LEAK.
    /// </summary>
    public static event Action<double>? OnAnimateFrame;

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

    public static void StartAnimation()
    {
        lock (_timerLock)
        {
            if (_animationTimer is null)
            {
                _animationTimer = new System.Timers.Timer(_animationRefire * 1000);
                _animationTimer.Elapsed += AnimateFrame;
                _animationTimer.Start();
            }
        }
    }

    private static void AnimateFrame(object? sender, System.Timers.ElapsedEventArgs e)
    {
        //Execute all animations on the main thread!
        Dispatcher.UIThread.Post(() =>
        {
            _animationTime += _animationRefire;
            OnAnimateFrame?.Invoke(_animationTime);
        });
    }

    public static void StopAnimation()
    {
        lock (_timerLock)
        {
            if (_animationTimer is not null)
            {
                _animationTimer.Stop();
                _animationTimer = null;
            }
        }
    }
}
