/* Note: This class contains two classes! This is to play ball with the genrics system!
 * 
 */
using Avalonia.Controls;

namespace UI.ViewModels;

public abstract class SettingInput
{
    public string Label { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = [];
    public StringInputMethod InputType { get; set; } = StringInputMethod.None;

    public string Description = string.Empty;

    public Control? Input;

    public const int InputDistanceFromRight = 50;

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
