using Avalonia.Controls;
using Avalonia.Threading;
using Core;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using UI.ViewModels;

namespace UI;

public partial class FolderView : UserControl
{
    public FolderView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetach;
        DetachedFromLogicalTree += OnLeaveScope;
        SetDateRange(null, null);
        CombinationBox.IsChecked = false;
        UpdateUsers();
    }

    private void OnDetach(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        MainWindowViewModel.OnAnimateFrame -= AnimateUsers;
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        UpdateUsersLate();
        DBInteract.OnProgramAdded += UpdateUsersLate;
        ConnectedUserInfo.OnUserConnectionModified += OnUserModified;
        MainWindowViewModel.OnAnimateFrame += AnimateUsers;
    }

    private void AnimateUsers(double animationFrame)
    {
        foreach (var user in FolderViewData.UserMapping.Values)
        {
            user.Animate(animationFrame);
        }
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
        DBInteract.OnProgramAdded -= UpdateUsersLate;
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
        folderUser.IsHost = user.IsHost;
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

    private void LiveViewChecked()
    {
        LiveViewModel.IsLive = true;
        NetworkDataManager.Instance.HostSendLivePayload(true);
    }

    private void LiveViewUnchecked()
    {
        LiveViewModel.IsLive = false;
        NetworkDataManager.Instance.HostSendLivePayload(false);
    }


    public void SetDateRange(DateTime? date1, DateTime? date2)
    {
        Date1Text.Text = date1?.ToString("MMMM d, yyyy h:mm tt") ?? "∞";
        Date2Text.Text = date2?.ToString("MMMM d, yyyy h:mm tt") ?? "∞";
    }

    public void LiveCheckChanged(object? sender, bool isChecked)
    {
        if (isChecked)
            LiveViewChecked();
        else
            LiveViewUnchecked();
    }

    public void CombinedCheckChanged(object? sender, bool isChecked)
    {
        Debug.Log($"Combination box changed: {isChecked}");
        CombinationModel.IsCombination = isChecked;
    }
}