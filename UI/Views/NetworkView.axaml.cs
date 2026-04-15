using Avalonia.Controls;
using Avalonia.LogicalTree;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Sockets;
using UI.ViewModels;

namespace UI;

public partial class NetworkView : UserControl
{
    private Dictionary<ConnectionInfo, NetworkViewConnection> _connectionViews = [];
    private SettingInput? _portInput = null;
    private SettingInput? _ipInput = null;
    public NetworkView()
    {
        AttachedToLogicalTree += OnEnterScope;
        DetachedFromLogicalTree += OnLeaveScope;
        InitializeComponent();
        _portInput = AddTextbox(SettingInt.HostPort, "Port");
        _ipInput = AddTextbox(SettingString.HostIP, "Host IP Binding");
        PopulateConnections();
        UpdateHostConfig();
        UpdateHostToggleButtonName();
        UpdateOnlineText();
    }
    public SettingInput? AddTextbox<T>(T setting, string watermark) where T : Enum
    {
        //Bind input/format textbox
        SettingInput? input = UiSettings.GetInputField(setting);
        if (input is not null and SettingInputField<T> inputField)
        {
            if (input.Input is not null and TextBox textbox)
            {
                textbox.UseFloatingWatermark = true;
                textbox.Watermark = watermark;

                textbox.Margin = new(0);
                textbox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

                textbox.KeyDown += (s, e) =>
                {
                    if (e.Key == Avalonia.Input.Key.Enter)
                    {
                        input.OnEnterPressed();
                        e.Handled = true;
                    }
                };

                hostSettingsStackPannel.Children.Add(input.Input);
            }
            input.BindSettingChange();
            input.IsWritingActive = true;
        }
        return input;
    }
    private void ClientConnectionAdded(ConnectionInfo info)
    {
        PopulateConnections();//Someone to populate.
    }

    private void OnDisconnectFromHost(Guid guid, ConnectionInfo connectionInfo)
    {
        if (!_connectionViews.TryGetValue(connectionInfo, out NetworkViewConnection? networkViewConnection)) return;
        if (networkViewConnection is null) return;
        networkViewConnection.UpdateOnlineDot();
    }

    private void OnConnectToHost(Guid guid, ConnectionInfo connectionInfo)
    {
        if (!_connectionViews.TryGetValue(connectionInfo, out NetworkViewConnection? networkViewConnection)) return;
        if (networkViewConnection is null) return;
        networkViewConnection.ClearErrors();
        networkViewConnection.UpdateOnlineDot();
    }

    private void OnEnterScope(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ConfigManager.OnClientConnectionAdded += ClientConnectionAdded;
        ConfigManager.OnClientConnectionRemoved += ClientConnectionRemoved;
        ConfigManager.OnSettingChanged += OnSettingChanged;
        NetworkManager.Instance.OnNetworkError += OnNetworkError;
        MainWindowViewModel.OnAnimateFrame += OnAnimate;
        NetworkManager.Instance.OnDisconnectFromHost += OnDisconnectFromHost;
        NetworkManager.Instance.OnConnectToHost += OnConnectToHost;
        NetworkManager.Instance.OnSocketError += OnSocketError;
    }

    private void OnLeaveScope(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        //Prevent memory leaks!
        ConfigManager.OnClientConnectionAdded -= ClientConnectionAdded;
        ConfigManager.OnClientConnectionRemoved -= ClientConnectionRemoved;
        ConfigManager.OnSettingChanged -= OnSettingChanged;
        MainWindowViewModel.OnAnimateFrame -= OnAnimate;
        //False positive, ignoring.
#pragma warning disable CS8601
        NetworkManager.Instance.OnNetworkError -= OnNetworkError;
        NetworkManager.Instance.OnDisconnectFromHost -= OnDisconnectFromHost;
        NetworkManager.Instance.OnConnectToHost -= OnConnectToHost;
        NetworkManager.Instance.OnSocketError -= OnSocketError;
#pragma warning restore CS8601
    }

    private void OnSocketError(Guid guid, ConnectionInfo connectionInfo, SocketError error)
    {

        if (!_connectionViews.TryGetValue(connectionInfo, out NetworkViewConnection? networkViewConnection)) return;
        if (networkViewConnection is null) return;

        networkViewConnection.SetSocketError(error);
    }
    private void OnAnimate(double animtime)
    {
        foreach (NetworkViewConnection networkView in _connectionViews.Values)
        {
            networkView.Animate(animtime);
        }
    }
    private void OnNetworkError(Guid guid, NetworkErrorType type)
    {
        Client? client = NetworkManager.Instance.GuidToClient(guid);
        if (client is null) return;

        if (!_connectionViews.TryGetValue(client.ConnectionInfo, out NetworkViewConnection? networkViewConnection)) return;
        if (networkViewConnection is null) return;

        networkViewConnection.SetNetworkError(type);
    }

    private void OnSettingChanged(Enum setting)
    {
        UpdateHostConfig();
        UpdateHostToggleButtonName();
        UpdateOnlineText();
    }

    private void UpdateHostToggleButtonName()
    {
        ToggleHostButton.Content = ConfigManager.ReadSettingBool(SettingInt.IsHosting) == false ? "Start Host" : "Stop Host";
    }

    private void UpdateHostConfig()
    {
        string hostStatus = $"Host Alive: {NetworkManager.Instance.Host is not null}";
        if (NetworkManager.Instance.Host is not null)
        {
            hostStatus += $"\nClient Count: {NetworkManager.Instance.Host.ConnectedSessions}";
        }

        HostStatusLabel.Content = hostStatus;
    }

    private void ClientConnectionRemoved(ConnectionInfo info)
    {
        if (!_connectionViews.ContainsKey(info)) return;

        ConnectionPanel.Children.Remove(_connectionViews[info]);
        _connectionViews.Remove(info);
    }

    private void PopulateConnections()
    {
        foreach (var connection in ConfigManager.CurrentClientConnections)
        {
            if (_connectionViews.ContainsKey(connection)) continue;
            NetworkViewConnection view = new(connection);
            _connectionViews.Add(connection, view);
            ConnectionPanel.Children.Add(view);
        }
    }

    void ErrorSubmittingDisplay()
    {
        Console.WriteLine("failure to display the blah blah who cares");
    }

    private void TryAddConnection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    { 
        ///
        /// Heads up: This function uses GOTO's.
        ///

        string? input = AddConnectionInput.Text;
        if (input is null) goto ErrorSubmittingDisplay;
        string[] inputSplit = input.Split(":");
        if (inputSplit.Length != 3) goto ErrorSubmittingDisplay;
        ConnectionInfo.TryParse(inputSplit[0], inputSplit[1], inputSplit[2], out ConnectionInfo? connectionInfo);

        if (connectionInfo is null) goto ErrorSubmittingDisplay;

        ConfigManager.AddClientConnection(connectionInfo);
        ConfigManager.TrySaveToFile();
        return;
    ErrorSubmittingDisplay:
        ErrorSubmittingDisplay();
    }

    private void ToggleHostClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int isHosting = ConfigManager.ReadSetting(SettingInt.IsHosting);
        //Toggle.
        isHosting = (isHosting == 0) ? 1 : 0;
        ConfigManager.WriteSetting(SettingInt.IsHosting, isHosting);
    }

    public void CustomCheckbox_CheckedChanged(object? sender, bool isChecked)
    {
        ConfigManager.WriteSetting(SettingInt.NetworkDisabled, 
            ConfigManager.ReadSetting(SettingInt.NetworkDisabled) == 0? 1 : 0);
        UpdateOnlineText();
    }
    void UpdateOnlineText()
    {
        disconnectBox.IsChecked = !ConfigManager.ReadSettingBool(SettingInt.NetworkDisabled);
        if (disconnectBox.IsChecked)
            disconnectBox.Title = "Go Offline";
        else
            disconnectBox.Title = "Go Online";
    }
}