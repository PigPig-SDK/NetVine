using Infrastructure;
using System;
using System.Collections.Generic;

namespace UI.ViewModels;

public static class FolderViewData
{
    public static Dictionary<string, FolderUser> UserMapping = [];

    public static Action<User>? OnUserAdded;
    public static Action? OnSelectionUpdated;

    public static IEnumerable<string> SelectedUsers()
    {
        foreach (var user in UserMapping)
        {
            if (user.Value.IsSelected) yield return user.Key;
        }
    }

    public static IEnumerable<(string name, int index)> SelectedUsersWithIndex()
    {
        int index = 0;
        foreach (var user in UserMapping)
        {
            if (user.Value.IsSelected) yield return (user.Key, index);
            index++;
        }
    }
}
