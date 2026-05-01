
using Spectre.Console;

namespace CLI.Commands;

public class HelpCommand : Command
{
    public HelpCommand(App app) : base(app)
    {
        
    }

    public override string Name => "help";

    public override string Description => "Displays a list of available commands and their descriptions.";

    public override string Usage => "'help'";

    public override void Execute(string[] args)
    {
        var table = new Table();

        table.AddColumn("[bold]Command[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Usage[/]");

        foreach (var command in App.CommandList.Values)
        {
            table.AddRow(
                $"[green]{command.Name}[/]",
                $"[grey]{command.Description}[/]",
                $"[yellow]{command.Usage}[/]"
            );
        }

        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);
        table.Title("[bold white]Available Commands[/]");
        table.ShowRowSeparators();
        table.Width(App.TableWidth);
        AnsiConsole.Write(table);
    }
}
