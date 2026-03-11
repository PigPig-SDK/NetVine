using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModels;

public static class LiveViewModel
{
    private static bool _isLive;
    public static bool IsLive { get => _isLive; set 
        {
            _isLive = value;
            ViewChanged?.Invoke(_isLive);
        } }

    public static Action<bool>? ViewChanged { get; set; }
}
