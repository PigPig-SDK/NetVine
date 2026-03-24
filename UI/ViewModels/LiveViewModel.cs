using System;

namespace UI.ViewModels;

public static class LiveViewModel
{
    private static bool _isLive;
    public static bool IsLive { get => _isLive; set 
        {
            _isLive = value;
            ViewChangedEvent?.Invoke(_isLive);
        } }

    public static event Action<bool>? ViewChangedEvent;
}
