using System.Text.Json.Serialization;
namespace Infrastructure.Notifications;

public class Notification
{
    public NotificationPriority Priority { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
    public Notification()
    {
        Priority = NotificationPriority.Message;
        Message = string.Empty;
        Time = DateTime.Now;
    }
    public Notification(NotificationPriority priority, string message)
    {
        Priority = priority;
        Message = message;
        Time = DateTime.Now;
    }
    public Notification(NotificationPriority priority, string message, DateTime time)
    {
        Priority = priority;
        Message = message;
        Time = time;
    }
}
