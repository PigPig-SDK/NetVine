using CLI;

namespace CLI.Commands;
public class ConnectCommand : Command
{
    public ConnectCommand(App app) : base(app)
    {
    }

    public override string Name => "connect";

    public override string Description => "Connects to a host";

    public override string Usage => "'connect 192.167.0.4 1000 password'\nConnects to 192.167.0.4 on port 1000 with the password of password";

    public override void Execute(string[] args)
    {

    }
}
