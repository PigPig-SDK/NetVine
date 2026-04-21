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
        
    }
}
