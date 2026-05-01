namespace CLI.Commands;

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
