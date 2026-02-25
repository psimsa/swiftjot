using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SwiftJot.Services;
using SwiftJot.ViewModels;

namespace SwiftJot.Views;

public partial class MainWindow : Window
{
    private HotKeyService? _hotKeyService;
    private bool _forceClose;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        RegisterHotKey();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        _hotKeyService?.Dispose();
        base.OnClosing(e);
    }

    internal void Close(bool force)
    {
        _forceClose = force;
        Close();
    }

    private void RegisterHotKey()
    {
        if (!OperatingSystem.IsWindows()) return;

        var platformHandle = TryGetPlatformHandle();
        if (platformHandle is null) return;

        _hotKeyService = new HotKeyService();
        _hotKeyService.Register(platformHandle.Handle);
    }

    private void AddNote_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.AddNote();
    }

    private void DeleteNote_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.DeleteSelectedNote();
    }

    private async void Export_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var storageProvider = StorageProvider;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Jot",
            SuggestedFileName = vm.SelectedNoteTitle,
            FileTypeChoices =
            [
                new FilePickerFileType("Text Files") { Patterns = ["*.txt"] },
                new FilePickerFileType("Markdown Files") { Patterns = ["*.md"] }
            ],
            DefaultExtension = "txt"
        });

        if (file is not null)
        {
            var path = file.TryGetLocalPath();
            if (path is not null)
                vm.ExportSelectedNote(path);
        }
    }
}
