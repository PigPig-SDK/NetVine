using Avalonia.Media;

namespace UI.ViewModels;

public class GraphLedgendEntry
{
    public Avalonia.Media.IBrush Color { get; set; } = new SolidColorBrush(Colors.White);
    public string Title { get; set; } = string.Empty;
}
