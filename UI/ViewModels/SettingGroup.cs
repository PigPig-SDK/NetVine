using System.Collections.Generic;

namespace UI.ViewModels;

public class SettingGroup
{
    public string Label { get; set; } = string.Empty;
    public List<SettingField> Fields = [];

    public SettingGroup(string label, List<SettingField> fields)
    {
        Label = label;
        Fields = fields;

    }
}
