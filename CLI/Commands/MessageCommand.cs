using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using Infrastructure.Notifications;
using Spectre.Console;

namespace CLI.Commands;

public class MessageCommand : Command
{
    public MessageCommand(App app) : base(app) { }

    public override string Name => "msg";

    public override string Description => "Sends a message to all immediate connections.";

    public override string Usage => "'msg Text of any shape afterwards'";

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
        var data = Packet.CreatePacket(new StringPayload(message)).ToBytes();
        NetworkManager.Instance.SendToAll(data);
        AnsiConsole.MarkupLineInterpolated($"Sent message.");
    }
}