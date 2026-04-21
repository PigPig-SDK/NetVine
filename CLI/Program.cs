using CLI;
using CLI.Commands;
using Infrastructure;

Netvine.Startup();
Console.Title = $"{Netvine.AppName} v{Netvine.Version} - CLI";

//https://spectreconsole.net/console/tutorials/creating-custom-renderables-tutorial
//Use this for all drawing

var app = new App()
    .AddCommand<ConnectCommand>()
    .AddCommand<DisconnectCommand>()
    .AddCommand<ClearCommand>()
    .AddCommand<ListCommand>()
    .AddCommand<HelpCommand>();


app.PrintIntro();
app.Run();
