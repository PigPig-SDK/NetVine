using Infrastructure.Notifications;
using Spectre.Console;

namespace CLI.Commands;

public class NotificationsCommand : Command
{
    public NotificationsCommand(App app) : base(app) { }

    public override string Name => "note";

    public override string Description => "Notification actions";

    public override string Usage => "'note' 'note clear'";

    public override void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            App.SystemMessage($"[{NotificationManager.Notifications.Count} Messages] - Printing notifications:");
            foreach (Notification notification in NotificationManager.Notifications)
            {
                AnsiConsole.MarkupLineInterpolated($"{notification.Time} : {notification.Priority} : {notification.Message}");

            }
        }
        else
        {
            switch (args[0])
            {
                case "clear":
                    NotificationManager.ClearMessages();
                    App.SystemMessage("Cleared messages");
                    break;
            }
        }
    }
}
