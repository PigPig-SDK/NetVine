using Avalonia.Controls;
using Avalonia.Threading;
using Core;
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
        AttachedToVisualTree += OnAttached;
        DetachedFromLogicalTree += OnLeaveScope;
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        UpdateUsersLate();
        DBInteract.OnDataAdded += UpdateUsersLate;
    }

    private void UpdateUsersLate() => Dispatcher.UIThread.Post(() => { UpdateUsers(); });

    /// <summary>
    /// Called when the item is removed from the view/simulation
    /// </summary>
    /// <param name="_">Discarded</param>
    /// <param name="__">Discarded</param>
    private void OnLeaveScope(object? _, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs __)
    {
        DBInteract.OnDataAdded -= UpdateUsersLate;
    }

    ~FolderView()
    {
        //Just incase...
        OnLeaveScope(null, null!);
    }

    private void AddUser(User user)
    {
        OnUserAdded?.Invoke(user);
        FolderUser folderUser = new(user);
        ScrollView.Children.Add(folderUser);
        _userMapping.Add(user.Username, folderUser);
    }

    public void UpdateUsers()
    {
        if (!_userMapping.ContainsKey(SystemHistory.Instance.SystemName))//Add ourselves!
        {
            User user = new User()
            {
                Username = SystemHistory.Instance.SystemName,
                IsOnline = true
            };
            AddUser(user);
        }

        List<User> users = DBInteract.GetUsers();
        foreach (User user in users)
        {
            if (_userMapping.ContainsKey(user.Username))
            {
                _userMapping[user.Username].IsOnline = SystemHistory.Instance.SystemName.Equals(user.Username) ? true : user.IsOnline;
            }
            else
            {
                AddUser(user);
            }
        }
    }
}