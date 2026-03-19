using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Infrastructure;
using System;
using System.Collections.Generic;
using UI.ViewModels;
using UI.Views;

namespace UI;

public partial class SettingsView : UserControl
{
    /// <summary>
    /// This contains all settings for the application to display. Add options to this if you want them to appear as editable
    /// </summary>
    private List<SettingGroup> _allSettings => new()
    {
        new SettingGroup("Application", [
            new SettingInputReporter($"Net-Vine version {Program.Version}",
                "This is your current netvine version.",
                "version", "app", "application"),

            new SettingInputCheckbox<Enum>($"Minimize on close",
                "When you close net-vine, should it remain silent in the background?",
                SettingInt.MinimizeOnClose,
                "minimize", "app", "application"),

            new SettingInputCheckbox<Enum>($"Load on startup",
                "Should netvine start automatically with your OS?",
                SettingInt.LoadOnStartup,
                "load", "startup", "app", "application"),
            ]),

        new SettingGroup ("Network",[
            new SettingInputfield<SettingInt>("Port for host",
                "The port used for hosting a server",
                StringInputMethod.IntInput,
                SettingInt.HostPort,
                "network", "host") { MaxAcceptedNumericalSize = 65535},

            new SettingInputfield<SettingString>("Ip for host",
                "The ip used for hosting a server",
                StringInputMethod.IntInput,
                SettingString.HostIP,
                "network", "host"),

            new SettingInputfield<SettingFloat>("Auto establish connection rate",
                "[In Seconds] How often the application will probe all programs on your system. If we are giving you tremendous overhead, try lowering this value.",
                StringInputMethod.FloatInput,
                SettingFloat.TickRate,
                "network", "usage", "interval"),

            new SettingInputCheckbox<SettingInt>("Auto establish connection on startup",
                "When re-establishing connection, how often do we check?",
                SettingInt.AutoEstablishConnection,
                "network", "client"),
        ]),
        new SettingGroup ("Database Storage", [

            new SettingInputCheckbox<SettingInt>("Track CPU usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackCPUUsage,
                "cpu", "usage", "db", "database"),

            new SettingInputCheckbox<SettingInt>("Track memory usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackMemoryUsage,
                "ram", "usage", "db", "database"),

            new SettingInputCheckbox<SettingInt>("Track network usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackNetworkUsage,
                "network", "usage", "db", "database"),

            new SettingInputCheckbox<SettingInt>("Track disk usage",
                "Enables/Disables tracking for this resource",
                SettingInt.TrackDiskUsage,
                "disk", "usage", "db", "database"),

            new SettingInputfield<SettingFloat>("Database save interval",
                "[In Seconds] How often the application will write to your database.",
                StringInputMethod.FloatInput,
                SettingFloat.DatabaseSaveInterval,
                "database", "usage", "interval", "db", "database"),

            new SettingInputfield<SettingFloat>("Probe rate",
                "[In Seconds] How often the application will probe all programs on your system. If we are giving you tremendous overhead, try lowering this value.",
                StringInputMethod.FloatInput,
                SettingFloat.TickRate,
                "memory", "usage", "interval", "db", "database"),
        ]),
    };

    public SettingsView()
    {
        InitializeComponent();
        MainWindowViewModel.OnSearchKeyStroke += SearchBarUpdated;
        SetupTree();
    }

    private void SearchBarUpdated(string? input)
    {
        if (MainWindowViewModel.ActiveTab != MainWindow.SettingsView) return;//Don't care.
        if (input == null) return;

        string[] split = input.Split(' ');//Split at spaces
        if (split.Length == 0) split = [input];
        SetupTree(split);
    }

    public void SetupTree(params string[] filterWords)
    {
        SettingsTreeView.Items.Clear();

        foreach (SettingGroup group in _allSettings)
        {
            var groupItem = new TreeViewItem
            {
                Header = new TextBlock { Text = group.Label },
                IsExpanded = true
            };
            foreach (SettingInput field in group.Fields)
            {
                if (!field.IsPartOfFilter(filterWords)) continue;

                //Setup tree item content
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                var label = new TextBlock { Text = field.Label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
                grid.Children.Add(label);

                Grid.SetColumn(label, 0);
                if (field.Input != null)
                {
                    field.Input.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Grid.SetColumn(field.Input, 1);
                    grid.Children.Add(field.Input);
                }

                //Setup the tree item
                ToolTip.SetTip(grid, field.Description);
                TreeViewItem treeItem = new() { Header = grid };
                treeItem.KeyDown += (s, e) => {
                    if (e.Key == Avalonia.Input.Key.Enter)
                    {
                        field.OnEnterPressed();
                        e.Handled = true;//Shut up...
                    }
                    };

                groupItem.Items.Add(treeItem);
            }
            SettingsTreeView.KeyDown += IgnoreEnterButton;
            //Don't add empty items.
            if (groupItem.Items.Count <= 0)
            {
                continue;
            }
            SettingsTreeView.Items.Add(groupItem);
        }
    }

    private void IgnoreEnterButton(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter) e.Handled = true;
    }
}