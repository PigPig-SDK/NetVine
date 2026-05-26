using Core;
using Infrastructure.Networking;
using Infrastructure.Notifications;

namespace Infrastructure;

public static class Netvine
{
    public const string Version = "0.0.1";
    public const string AppName = "Net-Vine";

    public static void Startup()
    {
        // Load configuration
        ConfigManager.Initialize();

        // Setup notification
        NotificationManager.Initilaize();

        // Setup history tracker.
        SystemHistory.SetupInstance();

        // Emplace DB
        DBInteract.Initialize();

        // Setup network manager
        NetworkManager.SetupInstance();

        //Setup live data manager
        NetworkDataManager.SetupInstance();

        //OS startup binding configuration
        SystemStartupBinder.Setup();

        //Notification Tracker setup last. Timings are not important
        NotificationTracker.Setup();


        PythonScriptLoader.Setup();

        ApiTracker.Setup();
        
    }
    public static void Shutdown()
    {
        ConfigManager.TrySaveToFile();

        NotificationManager.TrySaveToFile();

        NetworkManager.Instance.Disconnect();

        ApiTracker.Shutdown();

        PythonScriptLoader.Instance.Dispose();
    }
}
