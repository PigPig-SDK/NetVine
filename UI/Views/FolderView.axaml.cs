using Avalonia.Controls;
using Infrastructure;
using System;
using System.Collections.Generic;

namespace UI;

public partial class FolderView : UserControl
{
    private Dictionary<string, FolderUser> _userMapping = [];
    public static Action<User>? OnUserAdded;
    public static Action? OnSelectionUpdated;

    public FolderView()
    {
        InitializeComponent();
        UpdateUsers();
    }

    public void UpdateUsers()
    {
        List<User> users = DBInteract.GetUsers();
        foreach (User user in users)
        {
            if (_userMapping.ContainsKey(user.Username))
                continue;
            OnUserAdded?.Invoke(user);
            FolderUser folderUser = new(user);
            ScrollView.Children.Add(folderUser);
            _userMapping.Add(user.Username, folderUser);
        }
    }
}