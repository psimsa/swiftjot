using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using SwiftJot.Models;
using SwiftJot.Services;

namespace SwiftJot.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly StorageService _storageService;
    private readonly SettingsService _settingsService;
    private Note? _selectedNote;
    private Timer? _debounceTimer;
    private bool _isSettingsPanelVisible;
    private string _hotKeyStatusMessage = string.Empty;

    public ObservableCollection<Note> Notes { get; } = new();
    public AppSettings Settings { get; private set; }

    public Note? SelectedNote
    {
        get => _selectedNote;
        set
        {
            SetField(ref _selectedNote, value);
            OnPropertyChanged(nameof(SelectedNoteContent));
            OnPropertyChanged(nameof(SelectedNoteTitle));
        }
    }

    public string SelectedNoteContent
    {
        get => _selectedNote?.Content ?? string.Empty;
        set
        {
            if (_selectedNote is null) return;
            _selectedNote.Content = value;
            AutoUpdateTitleIfNeeded(value);
            ScheduleSave();
        }
    }

    public string SelectedNoteTitle
    {
        get => _selectedNote?.Title ?? string.Empty;
        set
        {
            if (_selectedNote is null) return;
            _selectedNote.Title = value;
            _selectedNote.HasManualTitle = true;
            OnPropertyChanged();
            ScheduleSave();
        }
    }

    public bool IsSettingsPanelVisible
    {
        get => _isSettingsPanelVisible;
        set => SetField(ref _isSettingsPanelVisible, value);
    }

    public bool CloseToTray
    {
        get => Settings.CloseToTray;
        set
        {
            Settings.CloseToTray = value;
            OnPropertyChanged();
            _settingsService.SaveSettings(Settings);
        }
    }

    public string HotKeyDisplayText => Settings.HotKey.ToDisplayString();

    public string HotKeyStatusMessage
    {
        get => _hotKeyStatusMessage;
        private set => SetField(ref _hotKeyStatusMessage, value);
    }

    public bool HasHotKeyStatusMessage => !string.IsNullOrEmpty(_hotKeyStatusMessage);

    public MainWindowViewModel()
    {
        _storageService = new StorageService();
        _settingsService = new SettingsService();
        Settings = _settingsService.LoadSettings();
        LoadNotes();
    }

    private void LoadNotes()
    {
        var notes = _storageService.LoadNotes();
        foreach (var note in notes)
            Notes.Add(note);

        if (Notes.Count == 0)
        {
            var defaultNote = new Note { Title = "My First Jot" };
            Notes.Add(defaultNote);
        }
        SelectedNote = Notes[0];
    }

    private void ScheduleSave()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            _storageService.SaveNotes([.. Notes]);
        }, null, 500, Timeout.Infinite);
    }

    public void AddNote()
    {
        var note = new Note { Title = $"Jot {Notes.Count + 1}" };
        Notes.Add(note);
        SelectedNote = note;
        ScheduleSave();
    }

    public void DeleteSelectedNote()
    {
        if (_selectedNote is null || Notes.Count <= 1) return;
        DeleteNote(_selectedNote);
    }

    public void DeleteNote(Note note)
    {
        if (Notes.Count <= 1) return;
        var idx = Notes.IndexOf(note);
        if (idx < 0) return;
        Notes.Remove(note);
        SelectedNote = Notes[Math.Max(0, idx - 1)];
        ScheduleSave();
    }

    public void ExportSelectedNote(string filePath)
    {
        if (_selectedNote is null) return;
        File.WriteAllText(filePath, _selectedNote.Content);
    }

    public void UpdateHotKey(HotKeyConfig config)
    {
        Settings.HotKey = config;
        _settingsService.SaveSettings(Settings);
        OnPropertyChanged(nameof(HotKeyDisplayText));
    }

    public void SetHotKeyStatusMessage(string message)
    {
        HotKeyStatusMessage = message;
        OnPropertyChanged(nameof(HasHotKeyStatusMessage));
    }

    public void ToggleSettingsPanel()
    {
        IsSettingsPanelVisible = !IsSettingsPanelVisible;
    }

    private void AutoUpdateTitleIfNeeded(string content)
    {
        if (_selectedNote is null || _selectedNote.HasManualTitle) return;

        var firstLine = content.Split('\n', 2)[0].Trim();
        if (firstLine.Length > 50) firstLine = firstLine[..50];
        var newTitle = firstLine.Length > 0 ? firstLine : $"Jot {Notes.IndexOf(_selectedNote) + 1}";

        if (_selectedNote.Title == newTitle) return;
        _selectedNote.Title = newTitle;
        OnPropertyChanged(nameof(SelectedNoteTitle));
    }
}
