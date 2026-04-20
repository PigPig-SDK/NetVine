using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Infrastructure;
using Infrastructure.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.ViewModels;

namespace UI.Views;


public partial class MainWindow : Window
{
    private Dictionary<int, Button> _tabBarMapping;
    public bool IsWindowInitialized = false;

    private int CurrentSidebarTab = 0;
    private bool isAnimatingSidebar = false;
    private const int SidebarMinSize = 40;

    public MainWindow()
    {
        _ = ResourceService.Instance;

        InitializeComponent();

        _tabBarMapping = new Dictionary<int, Button>() {
            { MainWindowViewModel.GraphView,  GraphButton},
            { MainWindowViewModel.TableView,TableButton },
            { MainWindowViewModel.SettingView, SettingsButton },
            { MainWindowViewModel.HomeView, HomeButton } };

        Width = ConfigManager.ReadSetting(SettingInt.WindowWidth);
        Height = ConfigManager.ReadSetting(SettingInt.WindowHeight);

        this.SizeChanged += OnSizeChanged;

        //Start with graph selected.
        CanvasTabControl.SelectedIndex = ConfigManager.ReadSetting(SettingInt.LastActivePage);//Only allow valid pages.
        SetTabSelected(CanvasTabControl.SelectedIndex);

        this.KeyDown += OnKeyDown;

        Opened += OnOpenedEvent;
        Closing += OnCloseEvent;
        SideBar.PropertyChanged += (s, e) =>
        {
            if (e.Property == BoundsProperty)
            {
                ExpanderLeft.IsVisible = SideBar.Bounds.Width != SidebarMinSize;
                ExpanderRight.IsVisible = SideBar.Bounds.Width == SidebarMinSize;
            }
        };
    }

    private void OnOpenedEvent(object? sender, EventArgs e)
    {
        NetworkDataManager.Instance.HostSendLivePayload(LiveViewModel.IsLive);

        if (!IsWindowInitialized && ConfigManager.ReadSettingBool(SettingInt.StartMinimized))
        {
            ShowInTaskbar = false;
            Hide();
        }
        IsWindowInitialized = true;
        MainWindowViewModel.StartAnimation();
    }

    private void OnCloseEvent(object? sender, WindowClosingEventArgs e)
    {
        NetworkDataManager.Instance.HostSendLivePayload(false);
        MainWindowViewModel.StopAnimation();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (ConfigManager.ReadSettingBool(SettingInt.MinimizeOnClose))
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
        CanvasTabControl.SelectedIndex = MainWindowViewModel.SettingView;
    }
    private void OnDudeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasTabControl.SelectedIndex = MainWindowViewModel.HomeView;
    }
    private void SetTabSelected(int tab)
    {
        if (tab != ConfigManager.ReadSetting(SettingInt.LastActivePage))//Tab has infact changed. 
            MainWindowViewModel.RaiseTabChanged(tab); //invokes OnTabChanged

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
            case MainWindowViewModel.TableView:
            case MainWindowViewModel.SettingView:
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

        SetTabSelected(CanvasTabControl.SelectedIndex);
    }

    private void SearchSubmission(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        MainWindowViewModel.OnSearchSubmission?.Invoke(e, SearchBoxInput.Text + e.Text);
    }

    private void SearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        MainWindowViewModel.OnSearchKeyStroke?.Invoke(SearchBoxInput.Text);
    }

    private void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TabControl tabControl)
        {
            // Run your code here based on which tab was clicked
            var clickedTab = e.AddedItems[0] as TabItem;

            if (clickedTab == QuickResize)
            {
                tabControl.SelectedIndex = CurrentSidebarTab;
                CurrentSidebarTab = 0;

                if(MainGrid.ColumnDefinitions[0].Width.Value == SidebarMinSize)
                    AnimateSidebar(SidebarMinSize, 250, 10);
                else
                    AnimateSidebar(MainGrid.ColumnDefinitions[0].Width.Value, SidebarMinSize, 10);
            }
            
        }
    }
    private void AnimateSidebar(double from, double to, int totalTicks)
    {
        if(isAnimatingSidebar) return;

        isAnimatingSidebar = true;
        var current = 0;
        var timer = new System.Timers.Timer(15);
        timer.Elapsed += (s, e) =>
        {
            var t = (double)current / totalTicks;
            t = t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
            var width = from + (to - from) * t;
            Dispatcher.UIThread.Post(() => MainGrid.ColumnDefinitions[0].Width = new GridLength(width));
            if (++current > totalTicks)
            {
                timer.Stop();
                isAnimatingSidebar = false;
            }
        };
        timer.Start();
    }
}