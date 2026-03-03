using Avalonia;
using Core;
using Infrastructure;
using Infrastructure.Networking;
using System;

namespace UI
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            //Step 1: Load configuration
            ConfigManager.Initialize();

            //Step 2: Emplace DB
            new DBInteract().Dispose();

            //Step 3: Setup history tracker.
            SystemHistory.SetupInstance();

            //Step 4: Setup network manager
            NetworkManager.SetupInstance();

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
}
