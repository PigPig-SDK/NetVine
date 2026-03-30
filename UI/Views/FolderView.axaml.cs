using Avalonia.Controls;
using Avalonia.Threading;
using Core;
using Infrastructure;
using Infrastructure.Networking.Packets;
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
        ConnectedUserInfo.OnUserConnectionModified += OnUserModified;
    }

    public void OnUserModified(string username, bool isAdded)
    {
        UpdateUsersLate();
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
        ConnectedUserInfo.OnUserConnectionModified -= OnUserModified;
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
        folderUser.IsOnline = user.IsOnline;
    }

    public void UpdateUsers()
    {
        List<User> users = DBInteract.ListAllUser();
        foreach (User user in users)
        {
            if (FolderViewData.UserMapping.ContainsKey(user.Username))
            {
                FolderViewData.UserMapping[user.Username].IsOnline = user.IsOnline;
            }
            else
            {
                AddUser(user);
            }
        }
    }

    private void LiveViewChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LiveViewModel.IsLive = true;
    }

    private void LiveViewUnchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LiveViewModel.IsLive = false;
    }
    
    public void SetDateRange(DateTime? date1, DateTime? date2)
    {
        String date1String = date1?.ToString("MMMM d, yyyy h:mm tt") ?? "inf";
        String date2String = date2?.ToString("MMMM d, yyyy h:mm tt") ?? "inf";
        
        DateRange.Text = date1String + " - \n" + date2String;
    }
}