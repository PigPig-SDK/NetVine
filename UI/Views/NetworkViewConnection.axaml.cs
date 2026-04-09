using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace UI;

public partial class NetworkViewConnection : UserControl
{
    private ConnectionInfo _connection;
    private Dictionary<NetworkErrorType, string> _connectionErrorText = [];
    private HashSet<SocketError> _socketErrors = [];

    private bool _isConnected = false;
    public bool IsConnected {
        get => _isConnected;
        set 
        {
            _isConnected = value;
            UpdateOnlineDot();
        } 
    }

    public NetworkViewConnection(ConnectionInfo connection)
    {
        InitializeComponent();
        this._connection = connection;
        Label.Content = $"{_connection.Ip}:{_connection.Port}:{_connection.Password}";
        UpdateOnlineDot();
    }

    private void DisconnectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ConfigManager.RemoveClientConnection(this._connection);
    }

    public void SetNetworkError(NetworkErrorType error)
    {
        switch (error)
        {
            case NetworkErrorType.None:
                {
                    _connectionErrorText.Clear();
                    break;
                }
            case NetworkErrorType.Unknown:
                _connectionErrorText.Add(error, "Unknown error"); break;
            case NetworkErrorType.BadPassword:
                _connectionErrorText.Add(error, "Bad password"); break;
        }
        UpdateOnlineDot();
    }

    private string ComputeConnectionErrorString() => $"{string.Join("\n", _connectionErrorText.Values)}{string.Join("\n", _socketErrors)}";

    private void UpdateOnlineDot()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ToolTip.SetTip(ActivityCircle, ComputeConnectionErrorString());
            ActivityCircle.Fill = IsConnected ? FolderUser.OnlineColor : FolderUser.OfflineColor;
            ActivityCircle.Stroke = IsConnected ? FolderUser.OfflineColor : FolderUser.OnlineColor;//Contrast, they are meant to be flipped.
        });
    }
    /// <summary>
    /// Important note. This is only to be called on main thread.
    /// </summary>
    /// <param name="time"></param>
    public void Animate(double time)
    {
        if (IsConnected)
        {
            ActivityCircle.StrokeThickness = 2;
            ActivityCircle.StrokeDashOffset = (time * 4);
        }
        else
        {
            ActivityCircle.StrokeThickness = MathF.Abs(MathF.Sin((float)time) * 2);
            ActivityCircle.StrokeDashOffset = MathF.Abs(MathF.Sin((float)time) * 2);
        }
    }

    internal void SetSocketError(SocketError error)
    {
        _socketErrors.Add(error);
        UpdateOnlineDot();
    }
}