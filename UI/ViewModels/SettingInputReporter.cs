using Avalonia.Controls;
using Infrastructure;

namespace UI.ViewModels;

public class SettingInputReporter : SettingInput
{
    public SettingInputReporter(string label, string discription, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        Description = discription;
    }

    public override void OnEnterPressed()
    {
    }
}
