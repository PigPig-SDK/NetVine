using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace UI;

public partial class FolderUser : UserControl
{
    public bool IsSelected = false;
    
    SolidColorBrush SelectedColor = new SolidColorBrush(Colors.DarkGreen);
    SolidColorBrush DeselectColor = new SolidColorBrush(Colors.Gray);

    public FolderUser(Infrastructure.User user)
    {
        InitializeComponent();
        UpdateVisual();
        UsernameLabel.Content = user.Username;
    }

    private void OnClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsSelected = !IsSelected;
        FolderView.OnSelectionUpdated?.Invoke();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if(IsSelected)
        {
            SelectionButton.Background = SelectedColor;
        }
        else
        {
            SelectionButton.Background = DeselectColor;
        }
    }
}