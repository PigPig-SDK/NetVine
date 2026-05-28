using Avalonia.Threading;
using Infrastructure.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UI.ViewModels
{
    public class LogViewModel
    {
        public ObservableCollection<Notification> LogEntries { get; } = new();

        private Predicate<Notification> _filterPredicate;

        public LogViewModel(Predicate<Notification> filter) {
            _filterPredicate = filter;
        }
        public void Refilter()
        {
            LogEntries.Clear();
            foreach (Notification notification in NotificationManager.Notifications.Reverse())
            {
                if (_filterPredicate(notification))
                    LogEntries.Add(notification);
            }
        }
        public void OnNotificationArrive(Notification notification)
        {
            //Filter uses UI, which must be done on main thread. Gag...
            Dispatcher.UIThread.Post(() =>
            {
                if (_filterPredicate(notification))
                    LogEntries.Add(notification);
            });
        }
    }
}
