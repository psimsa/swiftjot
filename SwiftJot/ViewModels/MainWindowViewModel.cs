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
    private Note? _selectedNote;
    private Timer? _debounceTimer;

    public ObservableCollection<Note> Notes { get; } = new();

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
            OnPropertyChanged();
            ScheduleSave();
        }
    }

    public MainWindowViewModel()
    {
        _storageService = new StorageService();
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
        var idx = Notes.IndexOf(_selectedNote);
        Notes.Remove(_selectedNote);
        SelectedNote = Notes[Math.Max(0, idx - 1)];
        ScheduleSave();
    }

    public void ExportSelectedNote(string filePath)
    {
        if (_selectedNote is null) return;
        File.WriteAllText(filePath, _selectedNote.Content);
    }
}
