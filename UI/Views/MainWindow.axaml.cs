using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Infrastructure;
using System;
using System.Collections.Generic;
using UI.ViewModels;
using YamlDotNet.Core.Events;

namespace UI.Views;


public partial class MainWindow : Window
{
    private Dictionary<int, Button> _tabBarMapping;

    public MainWindow()
    {
        InitializeComponent();

        _tabBarMapping = new Dictionary<int, Button>() { { MainWindowViewModel.GraphView,  GraphButton}, { MainWindowViewModel.TableView,TableButton } };

        Width = ConfigManager.ReadSetting(SettingInt.WindowWidth);
        Height = ConfigManager.ReadSetting(SettingInt.WindowHeight);

        this.SizeChanged += OnSizeChanged; 

        //Start with graph selected.
        CanvasTabControl.SelectedIndex = Math.Clamp(ConfigManager.ReadSetting(SettingInt.LastActivePage),0,1);//Only allow valid pages.
        SetTabSelected(CanvasTabControl.SelectedIndex);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var width = e.NewSize.Width;
        var height = e.NewSize.Height;

        ConfigManager.WriteSetting(SettingInt.WindowWidth, (int)width);
        ConfigManager.WriteSetting(SettingInt.WindowHeight, (int)height);
    }

    private void OnGraphClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = MainWindowViewModel.GraphView;
    }

    private void OnTableClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = MainWindowViewModel.TableView;
    }

    private void SetTabSelected(int tab)
    {
        if(tab != ConfigManager.ReadSetting(SettingInt.LastActivePage))//Tab has infact changed.
            MainWindowViewModel.OnTabChanged?.Invoke(tab);

        ConfigManager.WriteSetting(SettingInt.LastActivePage, tab);
        foreach (var tabButton in _tabBarMapping)
        {
            if(tabButton.Key == tab)
            {
                //Select
                tabButton.Value.Classes.Add("selected");
                tabButton.Value.Classes.Remove("deselected");
            }
            else
            {
                //Deselect
                tabButton.Value.Classes.Remove("selected");
                tabButton.Value.Classes.Add("deselected");
            }
        }
        
    }

    private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CanvasTabControl is null) return;//Use nullability people.

        int tab = CanvasTabControl.SelectedIndex;

        SetTabSelected(tab);
    }
}