using System.Collections.Generic;

namespace UI.ViewModels;

public class SettingGroup
{
    public string Label { get; set; } = string.Empty;
    public List<SettingInput> Fields = [];

    public SettingGroup(string label, List<SettingInput> fields)
    {
        Label = label;
        Fields = fields;

    }
}
