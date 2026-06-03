using Core;
using System.Text;

namespace Infrastructure;

public static class CrashHandler
{
    public static void Setup()
    {
        AppDomain.CurrentDomain.UnhandledException += OnException;
    }
    public static string DefaultFilePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine", "crashlogs");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
    private static void OnException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var fileName = $"crash_{timestamp}.log";
        var filePath = Path.Combine(DefaultFilePath, fileName);

        var content = new StringBuilder();
        content.AppendLine($"Timestamp:   {DateTime.Now}");
        content.AppendLine($"Fatal:       {e.IsTerminating}");
        content.AppendLine();
        content.AppendLine("=== Exception ===");
        content.AppendLine(exception?.ToString() ?? e.ExceptionObject?.ToString() ?? "Unknown error");

        // Walk inner exceptions
        var inner = exception?.InnerException;
        int depth = 1;
        while (inner != null)
        {
            content.AppendLine();
            content.AppendLine($"=== Inner Exception (depth {depth++}) ===");
            content.AppendLine(inner.ToString());
            inner = inner.InnerException;
        }

        //Print out the Debug log history
        content.AppendLine("=== Debug Logging history ===");
        foreach (string message in Debug.DebugMessages)
        {
            content.AppendLine(message);
        }

        File.WriteAllText(filePath, content.ToString());
    }
}
