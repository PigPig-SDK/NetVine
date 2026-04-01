using Avalonia.Controls;
using Avalonia.LogicalTree;
using Infrastructure;
using System;

namespace UI.ViewModels;

public class SettingInputCheckbox : SettingInput
{
    public SettingInt ConfigSetting { get; set; }

    public override Enum? GenericSetting => ConfigSetting;

    public SettingInputCheckbox(string label, string discription, SettingInt configSetting, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        ConfigSetting = configSetting;
        Description = discription;

        CheckBox checkbox = new();
        checkbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
        //Subscribe after change. Thank you.
        checkbox.IsCheckedChanged += CheckboxSubmission;
        checkbox.DetachedFromLogicalTree += TextboxDetachedFromLogicalTree;
        Input = checkbox;
        UpdateCheckbox();
    }

    private void TextboxDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        UnbindSettingChange();
    }

    private void UpdateCheckbox()
    {
        if(Input is CheckBox checkbox)
        {
            checkbox.IsChecked = ConfigManager.ReadSetting(ConfigSetting) == 1;
        }
    }
    private void CheckboxSubmission(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Input is not CheckBox checkbox) return;

        if (checkbox.IsChecked == null) return;//No idea...

        ConfigManager.WriteSetting((SettingInt)(object)ConfigSetting, checkbox.IsChecked.Value? 1 : 0);
        if(IsWritingActive)
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

    public override void BindSettingChange()
    {
        ConfigManager.OnSettingChanged += OnSettingChanged;
    }

    public override void UnbindSettingChange()
    {
        ConfigManager.OnSettingChanged -= OnSettingChanged;
    }
    private void OnSettingChanged(Enum setting)
    {
        if (setting is SettingInt settingOfType)
        {
            if (settingOfType.Equals(ConfigSetting))
            {
                UpdateCheckbox();
            }
        }
    }
}
