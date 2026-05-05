using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Notifications;

public class Notification
{
    public NotificationPriority Priority { get; }
    public string Message { get; }
    public Notification()
    {
        Priority = NotificationPriority.Message;
        Message = string.Empty;
    }
    public Notification(NotificationPriority priority, string message)
    {
        Priority = priority;
        Message = message;
    }


}
