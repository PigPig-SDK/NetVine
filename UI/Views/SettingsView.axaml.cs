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

        foreach (SettingGroup group in UiSettings.AllSettings)
        {
            var groupItem = new TreeViewItem
            {
                Header = new TextBlock { Text = group.Label },
                IsExpanded = true
            };
            foreach (SettingInput field in group.Fields)
            {
                if (!field.IsPartOfFilter(filterWords)) continue;
                field.BindSettingChange();
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
                field.IsWritingActive = true;//Fly away!!
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