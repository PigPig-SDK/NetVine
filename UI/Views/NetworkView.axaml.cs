using Avalonia.Controls;
using Avalonia.LogicalTree;
using Infrastructure;
using Infrastructure.Networking;
using System;
using System.Collections.Generic;
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

    private void OnEnterScope(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ConfigManager.OnClientConnectionAdded += ClientConnectionAdded;
        ConfigManager.OnClientConnectionRemoved += ClientConnectionRemoved;
        ConfigManager.OnSettingChanged += OnSettingChanged;
    }

    private void OnLeaveScope(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        ConfigManager.OnClientConnectionAdded -= ClientConnectionAdded;
        ConfigManager.OnClientConnectionRemoved -= ClientConnectionRemoved;
        ConfigManager.OnSettingChanged -= OnSettingChanged;
    }

    private void OnSettingChanged(Enum setting)
    {
        UpdateHostConfig();
    }

    private void UpdateHostConfig()
    {
        string hostStatus = $"Host Alive: {NetworkManager.Instance.Host is not null}";
        if(NetworkManager.Instance.Host is not null)
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
        foreach (var connection in ConfigManager.ClientConnections)
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
        if (inputSplit.Length != 2) goto ErrorSubmittingDisplay;
        ConnectionInfo.TryParse(inputSplit[0], inputSplit[1], out ConnectionInfo? connectionInfo);

        if (connectionInfo is null) goto ErrorSubmittingDisplay;

        ConfigManager.AddClientConnection(connectionInfo);
        return;
    ErrorSubmittingDisplay:
        ErrorSubmittingDisplay();
    }
}