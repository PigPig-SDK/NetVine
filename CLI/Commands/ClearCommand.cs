using Spectre.Console;

namespace CLI.Commands;

public class ClearCommand : Command
{
    public ClearCommand(App app) : base(app) { }

    public override string Name => "clear";

    public override string Description => "Clears the terminal";

    public override string Usage => "'clear'";

    public override void Execute(string[] args)
    {
        AnsiConsole.Clear();
        App.PrintIntro();
    }
}

public class QuitCommand : Command
{
    public QuitCommand(App app) : base(app) { }

    public override string Name => "quit";

    public override string Description => "Closes the application.";

    public override string Usage => "'quit'";

    public override void Execute(string[] args)
    {
        App.IsRunning = false;
    }
}
