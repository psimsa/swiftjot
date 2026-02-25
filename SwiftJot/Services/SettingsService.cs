using System;
using System.IO;
using System.Text.Json;
using SwiftJot.Models;

namespace SwiftJot.Services;

public class SettingsService
{
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SwiftJot");

    private static readonly string SettingsFilePath = Path.Combine(DataDirectory, "config.json");

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
                return new AppSettings();

            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.AppSettings) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            var json = JsonSerializer.Serialize(settings, AppJsonSerializerContext.Default.AppSettings);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch
        {
            // Silently fail - best-effort persistence
        }
    }
}
