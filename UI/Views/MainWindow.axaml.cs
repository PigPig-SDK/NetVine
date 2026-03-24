using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using UI.ViewModels;
using YamlDotNet.Core.Events;

namespace UI.Views;


public partial class MainWindow : Window
{
    private Dictionary<int, Button> _tabBarMapping;

    //Not an ENUM, these values respond to the tab of the CanvasTabControl.
    public const int GraphView = 0;
    public const int TableView = 1;
    public const int SettingsView = 2;

    public MainWindow()
    {
        InitializeComponent();

        //_tabBarMapping = new Dictionary<int, Button>() { { MainWindowViewModel.GraphView,  GraphButton}, { MainWindowViewModel.TableView,TableButton } }; old code from merge conflict
        _tabBarMapping = new Dictionary<int, Button>() { { GraphView, GraphButton }, { TableView, TableButton }, { SettingsView, SettingsButton } };

        Width = ConfigManager.ReadSetting(SettingInt.WindowWidth);
        Height = ConfigManager.ReadSetting(SettingInt.WindowHeight);

        this.SizeChanged += OnSizeChanged;

        //Start with graph selected.
        CanvasTabControl.SelectedIndex = ConfigManager.ReadSetting(SettingInt.LastActivePage);//Only allow valid pages.
        SetTabSelected(CanvasTabControl.SelectedIndex);

        this.KeyDown += OnKeyDown;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (ConfigManager.ReadSetting(SettingInt.MinimizeOnClose) == 1)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }
    }
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        //CTRL + F...
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            if (SearchBoxInput.IsVisible)
            {
                SearchBoxInput.Focus();
                SearchBoxInput.SelectAll();
            }
            e.Handled = true;
        }
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
    private void OnSettingsClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = SettingsView;
    }

    private void SetTabSelected(int tab)
    {
        //if(tab != ConfigManager.ReadSetting(SettingInt.LastActivePage))//Tab has infact changed. old code from merge conflict
        //    MainWindowViewModel.OnTabChanged?.Invoke(tab);
        MainWindowViewModel.ActiveTab = tab;

        SearchBoxInput.Text = string.Empty;

        ConfigManager.WriteSetting(SettingInt.LastActivePage, tab);
        foreach (var tabButton in _tabBarMapping)
        {
            if (tabButton.Key == tab)
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

        switch (tab)
        {
            case TableView:
            case SettingsView:
                SearchBoxPanel.IsVisible = true;
                break;
            default:
                SearchBoxPanel.IsVisible = false;
                break;
        }

    }

    private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CanvasTabControl is null) return;//Use nullability people.

        int tab = CanvasTabControl.SelectedIndex;

        SetTabSelected(tab);
    }

    private void SearchSubmission(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        MainWindowViewModel.OnSearchSubmission?.Invoke(e, SearchBoxInput.Text + e.Text);
    }

    private void SearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        MainWindowViewModel.OnSearchKeyStroke?.Invoke(SearchBoxInput.Text);
    }
}