using CLI;
using CLI.Commands;
using Core;
using Infrastructure;

Debug.ConsoleEnabled = false;
Netvine.Startup();
Console.Title = $"{Netvine.AppName} v{Netvine.Version} - CLI";

//https://spectreconsole.net/console/tutorials/creating-custom-renderables-tutorial
//Use this for all drawing

var app = new App()
    .AddCommand<ConnectCommand>()
    .AddCommand<DisconnectCommand>()
    .AddCommand<ClearCommand>()
    .AddCommand<RefreshCommand>()
    .AddCommand<ListCommand>()
    .AddCommand<QuitCommand>()
    .AddCommand<ConfigCommand>()
    .AddCommand<ViewCommand>()
    .AddCommand<HelpCommand>();


app.PrintIntro();
app.Run();
