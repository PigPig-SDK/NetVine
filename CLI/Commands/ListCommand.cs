using Infrastructure;
using Infrastructure.Networking;
using Spectre.Console;

namespace CLI.Commands;

public class ListCommand : Command
{
    public ListCommand(App app) : base(app) { }

    public override string Name => "list";

    public override string Description => "Lists a user configuration, or all configurations.";

    public override string Usage => "'list', 'list connections', 'list int', 'list float', 'list string'";

    public override void Execute(string[] args)
    {
        if(args.Length == 0)
        {
            ListString();
            ListFloat();
            ListInt();
            ListConnections();
            return;
        }
        
        switch (args[0])
        {
            case "string":
                ListString();
                break;
            case "float":
                ListFloat();
                break;
            case "int":
                ListFloat();
                break;
            case "connections":
                ListConnections();
                break;
            default:
                ListString(args[0]);
                ListFloat(args[0]);
                ListInt(args[0]);
                break;
        }
    }

    public void ListString(string? searchWord = null) => ListSettings<SettingString>("String Settings", searchWord, ConfigManager.ReadSetting);
    public void ListInt(string? searchWord = null) => ListSettings<SettingInt>("Int Settings", searchWord,s => ConfigManager.ReadSetting(s).ToString());
    public void ListFloat(string? searchWord = null) => ListSettings<SettingFloat>("Float Settings", searchWord, s => ConfigManager.ReadSetting(s).ToString());

    public void ListSettings<TEnum>(string title, string? keyword, Func<TEnum, string?> getValue) where TEnum : struct, Enum
    {
        var table = new Table();
        table.Title($"[bold white]{title}[/]");
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);
        table.ShowRowSeparators();

        table.AddColumn("[bold]Setting[/]");
        table.AddColumn("[bold]Value[/]");

        bool hasEntry = false;

        foreach (TEnum setting in Enum.GetValues<TEnum>())
        {
            if(keyword is not null && !setting.ToString().ToLower().Contains(keyword, StringComparison.OrdinalIgnoreCase)) continue;

            hasEntry = true;
            table.AddRow(
                $"[green]{setting}[/]",
                $"[grey]{getValue(setting)}[/]"
            );
        }
        if (!hasEntry) return;//Don't print empty tables
        table.Width(App.TableWidth);
        AnsiConsole.Write(table);
    }
    public void ListConnections()
    {
        var table = new Table();
        table.Title($"[bold white]Connections[/]");
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);
        table.ShowRowSeparators();

        table.AddColumn("[bold]Address[/]");
        table.AddColumn("[bold]Password[/]");

        foreach (ConnectionInfo info in ConfigManager.CurrentClientConnections)
        {
            string color = (NetworkManager.Instance.IsOnline(info)) ? "green]": "red] !! (Offline) !! ";
            table.AddRow(
                $"[{color}{info.Ip}:{info.Port}[/]",
                $"[grey]{info.Password}[/]"
            );
        }

        table.Width(App.TableWidth);
        AnsiConsole.Write(table);
    }
}
