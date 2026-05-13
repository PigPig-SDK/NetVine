using Core;
using Infrastructure.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Views;

namespace UI.ViewModels;

public static class ToastServer
{
    public static void Initialize()
    {
        NotificationManager.OnNotified += OnNotified;
    }
    public static void Shutdown()
    {
        NotificationManager.OnNotified -= OnNotified;
    }
    private static void OnNotified(Notification notification)
    {
        ToastWindow toast = new(notification.Message);
        Task.Run(async () =>
        {
            await toast.ShowToast();
        });
    }
}
