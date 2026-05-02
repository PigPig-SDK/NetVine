using CLI.Commands;
using Infrastructure;
using Microsoft.Extensions.DependencyModel;
using Spectre.Console;

namespace CLI;

public class App
{
    public static bool IsRunning = false;

    private readonly Dictionary<string, Command> _commandList = [];
    public IReadOnlyDictionary<string, Command> CommandList => _commandList;

    public int TableWidth => AnsiConsole.Profile.Width - 4;

    public App AddCommand<T>() where T : Command
    {
        T? command = (T?)Activator.CreateInstance(typeof(T), this);
        if (command is null) throw new InvalidCastException($"Failed to create instance of command {typeof(T).Name}.");

        _commandList.TryAdd(command.Name, command);
        return this;
    }

    public App Execute(string command, string[] args)
    {
        if(_commandList.TryGetValue(command, out Command? commandBody))
            commandBody.Execute(args);

        return this;
    }

    public void Run()
    {
        if(IsRunning) throw new InvalidOperationException("Cannot run app twice.");
        IsRunning = true;
        while (IsRunning)
        {
            string[]? readLine = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (readLine == null || readLine.Length == 0) continue;

            string commandName = readLine[0];
            string[] args = readLine[1..];

            if (_commandList.TryGetValue(commandName, out Command? command))
                command.Execute(args);
            else
                PrintError("Unknown command", "Type 'help' for a list of available commands.");
        }
    }

    public void PrintError(string errorTitle, string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]{errorTitle}:[/]'{message}'");
    }
    public void SystemMessage(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold grey]Netvine:'{message}'[/]");
    }

    internal void PrintIntro() => SystemMessage($"{Netvine.AppName} v{Netvine.Version}");
}
