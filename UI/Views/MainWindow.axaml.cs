using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace UI.Views;


public partial class MainWindow : Window
{
    private Dictionary<int, Button> _tabBarMapping;

    public const int GraphView = 0;
    public const int TableView = 1;


    public MainWindow()
    {
        InitializeComponent();

        _tabBarMapping = new Dictionary<int, Button>() { { GraphView,  GraphButton}, {TableView,TableButton } };

        Width = 700;
        Height = 700;
        //Start with graph selected.
        CanvasTabControl.SelectedIndex = GraphView;
        SetTabSelected(CanvasTabControl.SelectedIndex);
    }

    private void OnGraphClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = GraphView;
    }

    private void OnTableClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = TableView;
    }

    private void SetTabSelected(int tab)
    {
        Console.WriteLine("Tab : " + tab);
        foreach(var tabButton in _tabBarMapping)
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

        SetTabSelected(CanvasTabControl.SelectedIndex);
    }
}