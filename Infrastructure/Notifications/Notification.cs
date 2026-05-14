using System.Reflection.Emit;
using System.Text.Json.Serialization;
namespace Infrastructure.Notifications;

public class Notification
{
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
    public string TimeDelta
    {
        get
        {
            var delta = DateTime.Now - Time;
            if (delta.TotalDays >= 1) return $"{(int)delta.TotalDays}d ago";
            if (delta.TotalHours >= 1) return $"{(int)delta.TotalHours}h ago";
            if (delta.TotalMinutes >= 1) return $"{(int)delta.TotalMinutes}m ago";
            return $"{(int)delta.TotalSeconds}s ago";
        }
    }
    [JsonIgnore]
    
    public bool LongTerm { get; set; }
    public string LevelColor => Priority switch
    {
        NotificationPriority.Critical => "#C88282",
        NotificationPriority.Alert => "#FFEE8C",
        NotificationPriority.Message => "#A5c882",
        _ => "#FFFFFF"
    };
    public Notification()
    {
        Priority = NotificationPriority.Message;
        Message = string.Empty;
        Title = string.Empty;
        Time = DateTime.Now;
    }
    public Notification(NotificationPriority priority, string title, string message)
    {
        Title = title;
        Priority = priority;
        Message = message;
        Time = DateTime.Now;
    }
    public Notification(NotificationPriority priority, string title, string message, DateTime time)
    {
        Title = title;
        Priority = priority;
        Message = message;
        Time = time;
    }
}
