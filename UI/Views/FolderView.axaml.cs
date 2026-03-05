using Avalonia.Controls;
using Avalonia.Threading;
using Core;
using Infrastructure;
using System;
using System.Collections.Generic;
using UI.ViewModels;

namespace UI;

public partial class FolderView : UserControl
{
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
        FolderViewData.OnUserAdded?.Invoke(user);
        FolderUser folderUser = new(user);
        ScrollView.Children.Add(folderUser);
        FolderViewData.UserMapping.Add(user.Username, folderUser);
    }

    public void UpdateUsers()
    {
        if (!FolderViewData.UserMapping.ContainsKey(SystemHistory.Instance.SystemName))//Add ourselves!
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
            if (FolderViewData.UserMapping.ContainsKey(user.Username))
            {
                FolderViewData.UserMapping[user.Username].IsOnline = SystemHistory.Instance.SystemName.Equals(user.Username) ? true : user.IsOnline;
            }
            else
            {
                AddUser(user);
            }
        }
    }
}