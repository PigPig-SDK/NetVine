using System;

namespace Core;

public static partial class Debug
{
    private static readonly DateTime _startTime = DateTime.Now;
    private static readonly string _logFile = System.IO.Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "debug.txt");

    //enable for checking the table view 
    public static bool ConsoleEnabled = true;
    public static bool FileEnabled = false;
    public static bool Enabled = true;

    public static void Log(string message, string? fileName = null)
    {
        if (!Enabled) return;
        var elapsed = DateTime.Now - _startTime;
        var line = $"[{DateTime.Now:HH:mm:ss\\:ms}] [+{elapsed:hh\\:mm\\:ss\\:ms}] {message}";
        if (ConsoleEnabled) Console.WriteLine(line);
        var file = fileName ?? _logFile;
        if (FileEnabled) System.IO.File.AppendAllText(file, line + Environment.NewLine);
    }

}
