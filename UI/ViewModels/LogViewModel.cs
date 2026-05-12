using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace UI.ViewModels
{
    public class LogViewModel
    {
        public ObservableCollection<LogEntry> LogEntries { get; } = new();


        public LogViewModel() {
            AddEntry("INFO", "hey dude whats up");
            AddEntry("WARN", "ram is 1 million dollars now");
            AddEntry("ERROR", "hey man... ram is 2 million dollars now");
        }
        public void AddEntry(string level, string message)
        {
            LogEntries.Add(new LogEntry
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Level = level,
                Message = message
            });
        }
    }
}
