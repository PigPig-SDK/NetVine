using Infrastructure.Notifications;
using Meziantou.Framework.Win32;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class PythonScriptLoader : IDisposable
{
    public static PythonScriptLoader Instance = null!;
    private readonly JobObject? _job;
    private readonly List<Process> _processes = new();
    private bool _disposed = false;

    public static void Setup()
    {
        Instance = new();
        //Create directories.
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine");
        Directory.CreateDirectory(folder);
        folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine", "scripts");
        Directory.CreateDirectory(folder);

        Instance.LoadAll(folder);
    }

    public PythonScriptLoader()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
        {
            _job = new();
            _job.SetLimits(new JobObjectLimits
            {
                Flags = JobObjectLimitFlags.KillOnJobClose
            });
        }
        else
        {
            NotificationManager.WriteNotification(new Notification(NotificationPriority.Alert, "OS Error!", $"Scripting only supported on windows"));
        }
    }

    public void LoadAll(string pluginsFolder)
    {
        foreach (var script in Directory.GetFiles(pluginsFolder, "*.py"))
            Load(script);
    }

    public void Load(string scriptPath)
    {
        if (_job is null)
        {
            NotificationManager.WriteNotification(new Notification(NotificationPriority.Critical, "Python script unable to load.", $"{scriptPath} could not load. Scripting only works on windows."));
            return;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"{scriptPath} {Environment.ProcessId}",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
#pragma warning disable CA1416 // Validate platform compatibility
        _job.AssignProcess(process);
#pragma warning restore CA1416 // Validate platform compatibility
        _processes.Add(process);
    }

    public void Dispose()
    {
        if(_job is null) return; //No job object means we didn't start any processes, so nothing to dispose of.

        if (_disposed) return;
        _disposed = true;

        foreach (var process in _processes)
        {
            if (!process.HasExited)
            {
                process.Kill();
                process.WaitForExit();
            }
            process.Dispose();
        }

#pragma warning disable CA1416 // Validate platform compatibility
        _job.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility
    }
}