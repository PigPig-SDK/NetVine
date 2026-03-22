using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Infrastructure;
using Infrastructure.Networking;

namespace UI;

public partial class NetworkViewConnection : UserControl
{
    private ConnectionInfo _connection;

    public NetworkViewConnection(ConnectionInfo connection)
    {
        InitializeComponent();
        this._connection = connection;

        Label.Content = $"{_connection.Ip}:{_connection.Port}";
    }

    private void DisconnectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ConfigManager.RemoveClientConnection(this._connection);
    }
}