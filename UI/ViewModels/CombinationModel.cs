using System;

namespace UI.ViewModels;

public static class CombinationModel
{
    private static bool _isCombination = false;
    public static bool IsCombination { get => _isCombination; set 
    {
        _isCombination = value;
        ViewChangedEvent?.Invoke(_isCombination);
    } }

    public static event Action<bool>? ViewChangedEvent;
}