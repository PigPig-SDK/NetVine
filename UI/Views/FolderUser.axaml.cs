using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using UI.ViewModels;

namespace UI;

public partial class FolderUser : UserControl
{
    public bool IsSelected = true;
    
    private readonly static SolidColorBrush _selectedColor = new(Color.Parse("#A5C882"));
    private readonly static SolidColorBrush _deselectColor = new (new Color(255, 82, 82, 86));
    private readonly static SolidColorBrush _onlineColor = new (Colors.White);
    private readonly static SolidColorBrush _offlineColor = new (Colors.Black);
    private readonly static SolidColorBrush _lightText = new (Colors.White);
    private readonly static SolidColorBrush _darkText = new (Color.Parse("#1E152A"));

    private double timeTick = 0;

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
        OnlineCircle.StrokeThickness = IsOnline ? 2 : 0;
        UsernameLabel.Foreground = IsSelected? _darkText : _lightText;

        ToolTip.SetTip(OnlineCircle, IsOnline ? "Online" : "Offline");
    }

    public void Animate(double time)
    {
        if (OnlineCircle is null) return;
        if (IsOnline == false) return;
        OnlineCircle.StrokeDashOffset = (time * 4);
    }
}