using Infrastructure;
using System;
using System.Collections.Generic;

namespace UI.ViewModels;

public static class FolderViewData
{
    public static Dictionary<string, FolderUser> UserMapping = [];
    public static Action<User>? OnUserAdded;
    public static Action? OnSelectionUpdated;
}
