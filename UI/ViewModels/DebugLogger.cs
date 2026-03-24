using System;

namespace UI.ViewModels
{
    public static class DebugLogger
    {
        private static readonly DateTime _startTime = DateTime.Now;
        private static readonly string _logFile = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "debug.txt");

        public static bool ConsoleEnabled = false;
        public static bool Enabled = false;

        public static void Log(string message, string? fileName = null)
        {
            if (!Enabled) return;
            var elapsed = DateTime.Now - _startTime;
            var line = $"[{DateTime.Now:HH:mm:ss\\:ms}] [+{elapsed:hh\\:mm\\:ss\\:ms}] {message}";
            if (ConsoleEnabled) Console.WriteLine(line);
            var file = fileName ?? _logFile;
            System.IO.File.AppendAllText(file, line + Environment.NewLine);
        }

    }
}
