using Avalonia;
using Core;
using Infrastructure;
using Infrastructure.Networking;
using System;

namespace UI;

internal sealed class Program
{
    public const string Version = "0.0.1";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Load configuration
        ConfigManager.Initialize();

        // Setup history tracker.
        SystemHistory.SetupInstance();

        // Emplace DB
        DBInteract.Initialize();

        // Setup network manager
        NetworkManager.SetupInstance();

        //Setup live data manager
        NetworkDataManager.SetupInstance();

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

