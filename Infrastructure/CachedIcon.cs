namespace Infrastructure;

public class CachedIcon
{
    public string ProcessName { get; set; } = "Unknown";
    public byte[] IconData { get; set; }
    public IconFileType IconFileType { get; set; }
    public CachedIcon() {
        IconData = [];
    }
    public CachedIcon(string processName, byte[] iconData, IconFileType fileType)
    {
        ProcessName = processName;
        IconData = iconData;
        IconFileType = fileType;
    }
}
