using Avalonia.Threading;
using Core;
using Infrastructure;
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
        if (ConfigManager.ReadSettingBool(SettingInt.DisableToastPopups)) return;
        Dispatcher.UIThread.Post(async () =>
        {
            ToastWindow toast = new(notification);
            await toast.ShowToast(ConfigManager.ReadSetting(SettingInt.NotificationTimeMS));
        });

    }
}
