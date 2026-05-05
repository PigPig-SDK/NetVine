using Infrastructure.Notifications;
using Spectre.Console;

namespace CLI.Commands;

public class NotifyCommand : Command
{
    public NotifyCommand(App app) : base(app) { }

    public override string Name => "notify";

    public override string Description => "Creates a notification";

    public override string Usage => "'notify Text of any shape afterwards'";

    public override void Execute(string[] args)
    {
        if(args.Length < 1)
        {
            App.PrintError(Name, Usage);
            return;
        }
        string message = string.Join(" ", args);
        NotificationManager.WriteNotification(
            new Notification(NotificationPriority.Message, message));
        AnsiConsole.MarkupLineInterpolated($"Sent message and saved.");
    }
}