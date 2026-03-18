/* Note: This class contains two classes! This is to play ball with the genrics system!
 * 
 */
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Infrastructure;
using System;
using System.Linq;

namespace UI.ViewModels;

public abstract class SettingField
{
    public string Label { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = [];
    public SettingInputMethod InputType { get; set; } = SettingInputMethod.None;

    public string Description = string.Empty;

    public Control? Input;

    public abstract void OnEnterPressed();

    public bool IsPartOfFilter(string[] filterWords)
    {
        if (filterWords == null) return true;
        if (filterWords.Length == 0) return true;//No filter...
        if (filterWords.Length == 1 && string.IsNullOrEmpty(filterWords[0])) return true;

        foreach(string word in filterWords)
        {
            if (string.IsNullOrEmpty(word)) continue;

            string checker = word.ToLower();
            foreach(string keyword in Keywords)
            {
                if(keyword.ToLower().Contains(checker)) return true;
            }
            if(Description.ToLower().Contains(checker)) return true;
            if(Label.ToLower().Contains(checker)) return true;
        }

        return false;
    }
}
public class SettingField<T> : SettingField where T : Enum
{
    public T ConfigSetting { get; set; }

    public int MaxInputSize = int.MaxValue - 1;
    public int MaxAcceptedNumericalSize = int.MaxValue;
    public int MinAcceptedNumericalSize = 0;

    public const int InputDistanceFromRight = 50;

    public SettingField(string label, string discription, SettingInputMethod inputType, T configSetting, params string[] keywords)
    {
        Label = label;
        Keywords = keywords;
        ConfigSetting = configSetting;
        Description = discription;
        InputType = inputType;

        switch (inputType)
        {
            case SettingInputMethod.FloatInput:
            case SettingInputMethod.StringInput:
            case SettingInputMethod.IntInput:

                string inputString = ComputeInitialInputString();

                TextBox textbox = new TextBox { Text = inputString, Width = 120 };
                textbox.AddHandler(InputElement.TextInputEvent, InputCatcher, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                textbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
                Input = textbox;
                break;
            case SettingInputMethod.Checkbox:
                CheckBox checkbox = new()
                {
                    IsChecked = (ConfigManager.ReadSetting((SettingInt)(object)ConfigSetting) == 1),
                };
                checkbox.Margin = new Avalonia.Thickness(0, 0, InputDistanceFromRight, 0);
                //Subscribe after change. Thank you.
                checkbox.IsCheckedChanged += CheckboxSubmission;
                Input = checkbox;
                break;
        }
    }
    private void InputCatcher(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        if (e.Text == null) return;

        if (Input is not TextBox textbox) return;
        if (textbox.Text == null) return;

        string requestedNewInput = textbox.Text + e.Text;

        if(e.Text.Length >= MaxInputSize + 1) e.Handled = true;

        switch (InputType)
        {
            case SettingInputMethod.StringInput:
                return;
            case SettingInputMethod.FloatInput:
                if(requestedNewInput.Count(c => c == '.') > 1) e.Handled = true;
                break;
            case SettingInputMethod.IntInput:
                if (e.Text.Any(char.IsDigit) == false) e.Handled = true;//Don't accept new input...
                //Exceeds numeric size
                if(int.TryParse(requestedNewInput, out int result) 
                    && ((result > MaxAcceptedNumericalSize) || (result < MinAcceptedNumericalSize))) e.Handled = true;
                break;
        }
    }

    private string ComputeInitialInputString()
    {
        if(ConfigSetting is SettingInt settingInt)
            return ConfigManager.ReadSetting(settingInt).ToString();
        else if(ConfigSetting is SettingFloat settingFloat)
            return ConfigManager.ReadSetting(settingFloat).ToString();
        else if (ConfigSetting is SettingString settingString)
            return ConfigManager.ReadSetting(settingString).ToString();

        return string.Empty;
    }

    private void CheckboxSubmission(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Input is not CheckBox checkbox) return;

        if (checkbox.IsChecked == null) return;//No idea...

        ConfigManager.WriteSetting((SettingInt)(object)ConfigSetting, checkbox.IsChecked.Value? 1 : 0);
        ConfigManager.TrySaveToFile();
    }

    private void TextboxSubmission(TextBox textBox)
    {
        if(textBox.Text == null) return;
        string textOut = textBox.Text;
        if (ConfigSetting is SettingInt settingInt)
        {
            if (int.TryParse(textOut, out int value))
                ConfigManager.WriteSetting(settingInt, value);
        }
        else if (ConfigSetting is SettingFloat settingFloat)
        {
            if(float.TryParse(textOut, out float value))
                ConfigManager.WriteSetting(settingFloat, value);
        }
        else if (ConfigSetting is SettingString settingString)
            ConfigManager.WriteSetting(settingString, textOut);

        ConfigManager.TrySaveToFile();
    }

    public override void OnEnterPressed()
    {
        if (Input == null) return;

        if(Input is CheckBox checkbox)
        {
            checkbox.IsChecked = !checkbox.IsChecked;
        }
        else if (Input is TextBox textbox)
        {
            TextboxSubmission(textbox);
        }
    }
}
