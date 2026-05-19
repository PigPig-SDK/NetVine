using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Infrastructure;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace UI.ViewModels;

public class SettingInputField<T> : SettingInput where T : Enum
{
    public T ConfigSetting { get; set; }

    public override Enum? GenericSetting => ConfigSetting;


    public SettingInputField(string label, string discription, StringInputMethod inputType, T configSetting, params string[] keywords) :
        this(label, discription, inputType, configSetting, int.MaxValue, 0, keywords) { }

    public SettingInputField(string label, string discription, StringInputMethod inputType, T configSetting, decimal maxAcceptedNumericalSize = decimal.MaxValue, decimal minAcceptedNumericalSize = 0, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        ConfigSetting = configSetting;
        Description = discription;
        InputType = inputType;

        string inputString = ComputeInitialInputString();

        TemplatedControl textbox;
        if (inputType == StringInputMethod.StringInput)
        {
            textbox = new TextBox { Text = inputString, Width = 120, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, };
        }
        else
        {
            textbox = new NumericUpDown
            {
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Value = decimal.Parse(inputString),
                Width = 120,
                ShowButtonSpinner = false,
                Minimum = minAcceptedNumericalSize,
                Maximum = maxAcceptedNumericalSize,
                FormatString = inputType == StringInputMethod.IntInput ? "0" : "0.0"
            };
        }


        textbox.DetachedFromLogicalTree += TextboxDetachedFromLogicalTree;
        textbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
        Input = textbox;
    }

    private void TextboxDetachedFromLogicalTree(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        UnbindSettingChange();
    }
    private string ComputeInitialInputString()
    {
        if (ConfigSetting is SettingInt settingInt)
            return ConfigManager.ReadSetting(settingInt).ToString();
        else if (ConfigSetting is SettingFloat settingFloat)
            return ConfigManager.ReadSetting(settingFloat).ToString();
        else if (ConfigSetting is SettingString settingString)
            return ConfigManager.ReadSetting(settingString).ToString();

        return string.Empty;
    }

    private void TextboxSubmission(TextBox textBox)
    {
        if (textBox.Text == null) return;
        string textOut = textBox.Text;
        if (ConfigSetting is SettingInt settingInt)
        {
            if (int.TryParse(textOut, out int value))
                ConfigManager.WriteSetting(settingInt, value);
        }
        else if (ConfigSetting is SettingFloat settingFloat)
        {
            if (float.TryParse(textOut, out float value))
                ConfigManager.WriteSetting(settingFloat, value);
        }
        else if (ConfigSetting is SettingString settingString)
            ConfigManager.WriteSetting(settingString, textOut);
        if (IsWritingActive)
            ConfigManager.TrySaveToFile();
    }

    private void NumericSubmission(NumericUpDown textBox)
    {
        if (textBox.Value == null) return;
        decimal? textOut = textBox.Value;
        if(textOut is null) return;

        if (ConfigSetting is SettingInt settingInt)
        {
            ConfigManager.WriteSetting(settingInt, (int)textOut);
        }
        else if (ConfigSetting is SettingFloat settingFloat)
        {
            ConfigManager.WriteSetting(settingFloat, (int)textOut);
        }
        else if (ConfigSetting is SettingString settingString)
            ConfigManager.WriteSetting(settingString, textOut.ToString()!);

        if (IsWritingActive)
            ConfigManager.TrySaveToFile();
    }

    private void OnSettingChanged(Enum setting)
    {
        if(setting is T settingOfType)
        {
            if(settingOfType.Equals(ConfigSetting))
            {
                if(Input is TextBox textbox)
                    textbox.Text = ComputeInitialInputString();
                else if(Input is NumericUpDown numericUpDown)
                    numericUpDown.Value = decimal.Parse(ComputeInitialInputString());
            }
        }
    }

    public override void OnEnterPressed()
    {
        if (Input == null) return;

        if (Input is TextBox textbox)
        {
            TextboxSubmission(textbox);
        }
        else if(Input is NumericUpDown updown)
        {
            NumericSubmission(updown);
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
}
