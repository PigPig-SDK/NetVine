using System.Collections.ObjectModel;
using System.Linq;
using Infrastructure.Notifications;

namespace UI.ViewModels
{
    public class LogViewModel
    {
        public ObservableCollection<Notification> LogEntries { get; } = new();


        public LogViewModel() {
            foreach (Notification notification in NotificationManager.Notifications.Reverse())
            {
                LogEntries.Add(notification);
            }
        }

    }
}
