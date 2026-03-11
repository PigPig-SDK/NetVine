using Core;
using System.Collections.Concurrent;

namespace Infrastructure.Networking.Packets;

/// <summary>
/// This class is used to track connected and disconnected users.
/// </summary>
/// <remarks>No logic related to user connection/disconnection is here!</remarks>
public static class ConnectedUserInfo
{
    private static ConcurrentDictionary<Guid, string> _usernames = [];

    public delegate void UsersModifiedHandler(string username, bool isAdded);
    public static UsersModifiedHandler? OnUserConnectionModified;

    public static bool IsUserConnected(string username) 
    {
        if (SystemHistory.Instance.SystemName.Equals(username)) return true;//We are always online... Even if there is no username.

        return _usernames.Values.Contains(username);
    }

    /// <summary>
    /// Removes the user from the connected users list.
    /// </summary>
    /// <remarks>Does not disconnect the user! Do not call this expecting a user to be disconnected</remarks>
    /// <param name="userId">The user's ID</param>
    /// <param name="username">The username</param>
    public static void RemoveUserData(Guid userId, out string? username)
    {
        _usernames.Remove(userId, out username);
        if(username != null)
            OnUserConnectionModified?.Invoke(username, false);
    }
    /// <summary>
    /// Adds user data to the usermap.
    /// </summary>
    public static void AddUserData(Guid userId, string username)
    {
        _usernames[userId] = username;
        if (username != null)
            OnUserConnectionModified?.Invoke(username, true);
    }
}
