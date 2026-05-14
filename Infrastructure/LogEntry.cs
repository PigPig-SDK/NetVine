using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string LevelColor => Level switch
        {
            "ERROR" => "#C88282",
            "WARN" => "#FFFFFF",
            "INFO" => "#A5c882",
            _ => "#FFFFFF"
        };
    }
}
