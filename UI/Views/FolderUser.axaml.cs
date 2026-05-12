using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Core;
using Infrastructure;
using Infrastructure.Networking;
using System;
using UI.ViewModels;

namespace UI;

public partial class FolderUser : UserControl
{
    public bool IsSelected = true;

    public readonly static SolidColorBrush SelectedColor = new(Color.Parse("#A5C882"));
    public readonly static SolidColorBrush DeselectColor = new(new Color(255, 82, 82, 86));
    public readonly static SolidColorBrush OnlineColor = new(Colors.White);
    public readonly static SolidColorBrush OfflineColor = new(Colors.Black);
    public readonly static SolidColorBrush LightText = new(Colors.White);
    public readonly static SolidColorBrush DarkText = new(Color.Parse("#1E152A"));

    private readonly User _user;

    public String Username => _user.Username;

    public FolderUser(User user)
    {
        InitializeComponent();
        UpdateVisual();
        UsernameLabel.Content = user.Username;
        _user = user;
    }

    private bool _isOnline = false;
    public bool IsOnline
    {
        get
        {
            return _isOnline;
        }
        set
        {
            _isOnline = value;
            UpdateVisual();
        }
    }

    private void OnClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsSelected = !IsSelected;
        FolderViewData.OnSelectionUpdated?.Invoke();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (SelectionButton is null || OnlineCircle is null) return;

        SelectionButton.Background = IsSelected ? SelectedColor : DeselectColor;
        OnlineCircle.Fill = IsOnline ? OnlineColor : OfflineColor;
        OnlineCircle.StrokeThickness = IsOnline ? 2 : 0;
        UsernameLabel.Foreground = IsSelected ? DarkText : LightText;

        ToolTip.SetTip(OnlineCircle, IsOnline ? "Online" : "Offline");
    }

    public void Animate(double time)
    {
        if (OnlineCircle is null) return;
        if (IsOnline == false) return;
        OnlineCircle.StrokeDashOffset = (time * 4);
    }

    private void CopyIP(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.Log("TODO: Implement user -> IP sync");
    }

    private void CopyUsername(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard is not null)
        {
            clipboard.SetTextAsync(_user.Username);
        }
    }
}