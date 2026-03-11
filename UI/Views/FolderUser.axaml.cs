using Avalonia.Controls;
using Avalonia.Media;
using System;
using UI.ViewModels;

namespace UI;

public partial class FolderUser : UserControl
{
    public bool IsSelected = false;
    
    private readonly static SolidColorBrush _selectedColor = new(Colors.DarkGreen);
    private readonly static SolidColorBrush _deselectColor = new SolidColorBrush(Colors.Gray);
    private readonly static SolidColorBrush _onlineColor = new SolidColorBrush(Colors.Green);
    private readonly static SolidColorBrush _offlineColor = new SolidColorBrush(Colors.Black);

    public FolderUser(Infrastructure.User user)
    {
        InitializeComponent();
        UpdateVisual();
        UsernameLabel.Content = user.Username;
    }

    private bool _isOnline = false;
    public bool IsOnline { 
        get { 
            return _isOnline;
        } 
        set { 
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

        SelectionButton.Background = IsSelected? _selectedColor : _deselectColor;
        OnlineCircle.Fill = IsOnline ? _onlineColor : _offlineColor;
    }
}