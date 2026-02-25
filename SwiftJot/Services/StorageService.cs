using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SwiftJot.Models;

namespace SwiftJot.Services;

public class StorageService
{
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SwiftJot");

    private static readonly string NotesFilePath = Path.Combine(DataDirectory, "notes.json");

    public List<Note> LoadNotes()
    {
        try
        {
            if (!File.Exists(NotesFilePath))
                return [];

            var json = File.ReadAllText(NotesFilePath);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.ListNote) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void SaveNotes(List<Note> notes)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            var json = JsonSerializer.Serialize(notes, AppJsonSerializerContext.Default.ListNote);
            File.WriteAllText(NotesFilePath, json);
        }
        catch
        {
            // Silently fail - best-effort persistence
        }
    }
}
