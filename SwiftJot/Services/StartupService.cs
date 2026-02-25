using System;
using System.IO;

namespace SwiftJot.Services;

public static class StartupService
{
    private static readonly string StartupFolder =
        Environment.GetFolderPath(Environment.SpecialFolder.Startup);

    private static readonly string ShortcutPath =
        Path.Combine(StartupFolder, "SwiftJot.lnk");

    public static bool IsRegistered => File.Exists(ShortcutPath);

    public static void Register()
    {
        var exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "SwiftJot.exe");
        // Write a startup .bat as an AOT-compatible alternative to COM-based .lnk creation
        var batPath = Path.ChangeExtension(ShortcutPath, ".bat");
        File.WriteAllText(batPath, $"@echo off\nstart \"\" \"{exePath}\"");
    }

    public static void Unregister()
    {
        if (File.Exists(ShortcutPath)) File.Delete(ShortcutPath);
        var batPath = Path.ChangeExtension(ShortcutPath, ".bat");
        if (File.Exists(batPath)) File.Delete(batPath);
    }
}
