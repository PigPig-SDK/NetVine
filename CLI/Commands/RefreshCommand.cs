using Infrastructure;

namespace CLI.Commands;

public class RefreshCommand : Command
{
    public RefreshCommand(App app) : base(app) { }

    public override string Name => "refresh";

    public override string Description => "Refreshes the current user config.";

    public override string Usage => "'refresh'";

    public override void Execute(string[] args)
    {
        ConfigManager.TryLoadFromFile();
        App.SystemMessage("Configuration refreshed.");
    }
}