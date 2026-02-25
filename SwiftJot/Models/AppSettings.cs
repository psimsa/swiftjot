namespace SwiftJot.Models;

public class AppSettings
{
    public bool CloseToTray { get; set; } = true;
    public HotKeyConfig HotKey { get; set; } = new HotKeyConfig();
}

public class HotKeyConfig
{
    public bool UseCtrl { get; set; } = true;
    public bool UseAlt { get; set; } = true;
    public bool UseShift { get; set; } = false;
    public uint KeyCode { get; set; } = 0x20; // VK_SPACE
    public string KeyDisplayName { get; set; } = "Space";

    public string ToDisplayString()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (UseCtrl) parts.Add("Ctrl");
        if (UseAlt) parts.Add("Alt");
        if (UseShift) parts.Add("Shift");
        parts.Add(KeyDisplayName);
        return string.Join(" + ", parts);
    }
}
