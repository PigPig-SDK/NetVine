using Infrastructure;
using Infrastructure.Networking;
using Spectre.Console;

namespace CLI.Commands;

public class DisconnectCommand : Command
{
    public DisconnectCommand(App app) : base(app)
    {
    }

    public override string Name => "disconnect";

    public override string Description => "Disconnects from the current session.";

    public override string Usage => "'disconnect'";

    public override void Execute(string[] args)
    {
        if (args.Length != 3)
        {
            if(ConfigManager.CurrentClientConnections.Count == 0)
            {
                App.PrintError("No active connections", "There are no active connections to disconnect from.");
                return;
            }

            Dictionary<string, ConnectionInfo> connections = ConfigManager.CurrentClientConnections.ToDictionary(c => c.ToString(), c => c);
            var prompt = new SelectionPrompt<string>()
            .Title("What [green]size pizza[/] would you like?");

            foreach(var connection in ConfigManager.CurrentClientConnections)
            {
                string connectionString = connection.ToString();
                connections.TryAdd(connectionString, connection);
                prompt.AddChoice(connectionString);
            }
            prompt.AddChoice("Close");
            string selection = AnsiConsole.Prompt(prompt);
            if (selection == "Close") return;

            connections.TryGetValue(selection, out ConnectionInfo? conTest);
            if(conTest is not null) NetworkManager.RemoveClientConnection(conTest);

            return;
        }

        if (int.TryParse(args[1], out int port) == false || port < 0 || port > 65535)
        {
            App.PrintError("Invalid port", $"Port must be a number between 0 and 65535.");
            return;
        }

        ConnectionInfo info = new ConnectionInfo(
            connection: args[0],
            port,
            password: args[2]);

        NetworkManager.RemoveClientConnection(info);
    }
}
