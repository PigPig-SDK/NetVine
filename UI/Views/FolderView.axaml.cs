using Avalonia.Controls;
using Avalonia.Threading;
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
        DBInteract.OnDataAdded += UpdateUsersLate;
        DetachedFromLogicalTree += OnLeaveScope;
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