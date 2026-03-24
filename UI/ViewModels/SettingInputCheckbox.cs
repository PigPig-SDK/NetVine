using Avalonia.Controls;
using Infrastructure;
using System;

namespace UI.ViewModels;

public class SettingInputCheckbox<T> : SettingInput where T : Enum
{
    public T ConfigSetting { get; set; }

    public SettingInputCheckbox(string label, string discription, T configSetting, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        ConfigSetting = configSetting;
        Description = discription;

        CheckBox checkbox = new()
        {
            IsChecked = (ConfigManager.ReadSetting((SettingInt)(object)ConfigSetting) == 1),
        };
        checkbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
        //Subscribe after change. Thank you.
        checkbox.IsCheckedChanged += CheckboxSubmission;
        Input = checkbox;
    }

    private void CheckboxSubmission(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Input is not CheckBox checkbox) return;

        if (checkbox.IsChecked == null) return;//No idea...

        ConfigManager.WriteSetting((SettingInt)(object)ConfigSetting, checkbox.IsChecked.Value? 1 : 0);
        ConfigManager.TrySaveToFile();
    }
    public override void OnEnterPressed()
    {
        if (Input == null) return;

        if(Input is CheckBox checkbox)
        {
            checkbox.IsChecked = !checkbox.IsChecked;
        }
    }
}
