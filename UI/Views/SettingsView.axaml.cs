using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Infrastructure;
using System;
using System.Collections.Generic;

namespace UI;

public enum SettingsType
{
    None,
    Checkbox,
    FloatInput,
    NumberInput,
    StringInput,
}

public class SettingField<T> where T : Enum
{
    public string Label { get; set; }
    public string Keywords { get; set; }

    private Control? Input;
    public T SettingType { get; set; } 

    public SettingField(string label, string keywords, SettingsType inputType)
    {
        Label = label;
        Keywords = keywords;
        
        switch(inputType)
        {
            case SettingsType.FloatInput:
            case SettingsType.StringInput:
            case SettingsType.NumberInput:
                Input = new TextBox { Text = "8080", Width = 120 };
                break;
            case SettingsType.Checkbox:
                Input = new CheckBox { IsChecked = true };
                break;
        }
    }
}

public class SettingGroup
{
    public string Label { get; set; }
    public List<SettingField> Fields { get; set; }
}

public partial class SettingsView : UserControl
{

    private List<SettingGroup> _allSettings = new()
    {
        new SettingGroup { Label = "Network", Fields = new()
        {
            new SettingField<SettingFloat> ("Port", "Network ", SettingsType.TextInput),
        }},
        new SettingGroup { Label = "GUI", Fields = new()
        {
            new SettingField ("DarkMode", "Network", SettingsType.Checkbox),
        }},
    }; 

    public SettingsView()
    {
        InitializeComponent();
    }

    public void SetupTree()
    {
        SettingsTreeView.Items.Clear();

    }
}