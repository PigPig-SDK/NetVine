using Avalonia.Controls;
using System;

namespace UI.ViewModels;

public class SettingInputReporter : SettingInput
{
    public SettingInputReporter(string label, string discription, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        Description = discription;
    }

    public override Enum? GenericSetting => null;

    public override void BindSettingChange()
    {
    }

    public override void OnEnterPressed()
    {
    }

    public override void UnbindSettingChange()
    {
    }
}
