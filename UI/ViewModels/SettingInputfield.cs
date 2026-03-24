using Avalonia.Controls;
using Avalonia.Input;
using Infrastructure;
using System;
using System.Linq;

namespace UI.ViewModels;

public class SettingInputField<T> : SettingInput where T : Enum
{
    public T ConfigSetting { get; set; }

    public int MaxInputSize = int.MaxValue - 1;
    public int MaxAcceptedNumericalSize = int.MaxValue;
    public int MinAcceptedNumericalSize = 0;

    public SettingInputField(string label, string discription, StringInputMethod inputType, T configSetting, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        ConfigSetting = configSetting;
        Description = discription;
        InputType = inputType;

        string inputString = ComputeInitialInputString();

        TextBox textbox = new TextBox { Text = inputString, Width = 120 };
        textbox.AddHandler(InputElement.TextInputEvent, InputCatcher, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        textbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
        Input = textbox;
    }
    private void InputCatcher(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        if (e.Text == null) return;

        if (Input is not TextBox textbox) return;
        if (textbox.Text == null) return;

        string requestedNewInput = textbox.Text + e.Text;

        if (e.Text.Length >= MaxInputSize + 1) e.Handled = true;

        switch (InputType)
        {
            case StringInputMethod.StringInput:
                return;
            case StringInputMethod.FloatInput:
                if (requestedNewInput.Count(c => c == '.') > 1) e.Handled = true;
                break;
            case StringInputMethod.IntInput:
                if (e.Text.Any(char.IsDigit) == false) e.Handled = true;//Don't accept new input...
                //Exceeds numeric size
                if (int.TryParse(requestedNewInput, out int result)
                    && ((result > MaxAcceptedNumericalSize) || (result < MinAcceptedNumericalSize))) e.Handled = true;
                break;
        }
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

        ConfigManager.TrySaveToFile();
    }

    public override void OnEnterPressed()
    {
        if (Input == null) return;

        if (Input is TextBox textbox)
        {
            TextboxSubmission(textbox);
        }
    }
}
