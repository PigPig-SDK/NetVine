using CLI;
using CLI.Commands;
using Infrastructure;

//https://spectreconsole.net/console/tutorials/creating-custom-renderables-tutorial
//Use this for all drawing

Netvine.Startup();

var app = new App()
    .AddCommand<ConnectCommand>()
    .AddCommand<DisconnectCommand>()
    .AddCommand<ClearCommand>()
    .AddCommand<HelpCommand>()
    .Execute("help", []);

app.Run();
