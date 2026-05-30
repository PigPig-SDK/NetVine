namespace UI.ViewModels;

public class PastelPalette
{
    private readonly ScottPlot.Palettes.Category20 _inner = new();

    public ScottPlot.Color GetColor(int index)
    {
        var c = _inner.GetColor(index);
        return new ScottPlot.Color(
            (byte)(c.R * 0.5 + 127),
            (byte)(c.G * 0.5 + 127),
            (byte)(c.B * 0.5 + 127),
            c.Alpha
        );
    }
}