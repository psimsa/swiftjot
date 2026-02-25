using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SwiftJot.Models;
using SwiftJot.Services;
using SwiftJot.ViewModels;

namespace SwiftJot.Views;

public partial class MainWindow : Window
{
    private HotKeyService? _hotKeyService;
    private bool _forceClose;
    private bool _recordingHotKey;

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
            var closeToTray = DataContext is MainWindowViewModel vm && vm.CloseToTray;
            if (closeToTray)
            {
                e.Cancel = true;
                Hide();
                return;
            }
        }
        _hotKeyService?.Dispose();
        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_recordingHotKey)
        {
            CaptureHotKey(e);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
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

        var config = DataContext is MainWindowViewModel vm ? vm.Settings.HotKey : null;
        _hotKeyService = new HotKeyService();
        var result = _hotKeyService.Register(platformHandle.Handle, config);

        if (result != HotKeyRegistrationResult.Success && DataContext is MainWindowViewModel vm2)
        {
            var msg = result == HotKeyRegistrationResult.Conflict
                ? "⚠ Hotkey is already in use by another application"
                : "⚠ Failed to register hotkey";
            vm2.SetHotKeyStatusMessage(msg);
        }
    }

    private void TryApplyHotKey(HotKeyConfig config)
    {
        if (!OperatingSystem.IsWindows()) return;

        var platformHandle = TryGetPlatformHandle();
        if (platformHandle is null) return;

        // Unregister the old hotkey before attempting the new one.
        _hotKeyService?.Dispose();
        _hotKeyService = new HotKeyService();
        var result = _hotKeyService.Register(platformHandle.Handle, config);

        if (DataContext is not MainWindowViewModel vm) return;

        if (result == HotKeyRegistrationResult.Success)
        {
            vm.UpdateHotKey(config);
            vm.SetHotKeyStatusMessage(string.Empty);
        }
        else
        {
            // New hotkey failed — restore the previous registration.
            _hotKeyService.Dispose();
            _hotKeyService = new HotKeyService();
            _hotKeyService.Register(platformHandle.Handle, vm.Settings.HotKey);

            var msg = result == HotKeyRegistrationResult.Conflict
                ? "⚠ Hotkey is already in use by another application"
                : "⚠ Failed to register hotkey";
            vm.SetHotKeyStatusMessage(msg);
        }
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

    private void DeleteNoteInline_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Note note } && DataContext is MainWindowViewModel vm)
            vm.DeleteNote(note);
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

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.ToggleSettingsPanel();
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        _forceClose = true;
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Close(true);
            desktop.Shutdown();
        }
    }

    private void RecordHotKey_Click(object? sender, RoutedEventArgs e)
    {
        _recordingHotKey = true;
        if (RecordHotKeyButton is not null)
            RecordHotKeyButton.Content = "Press keys...";
        Focus();
    }

    private void StopRecording()
    {
        _recordingHotKey = false;
        if (RecordHotKeyButton is not null)
            RecordHotKeyButton.Content = "Record";
    }

    private void CaptureHotKey(KeyEventArgs e)
    {
        var key = e.Key;

        // Keep recording while the user is still holding modifier keys.
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
                 or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
            return;

        // Escape cancels recording without applying any change.
        if (key is Key.Escape)
        {
            StopRecording();
            return;
        }

        StopRecording();

        var modifiers = e.KeyModifiers;
        var config = new HotKeyConfig
        {
            UseCtrl = modifiers.HasFlag(KeyModifiers.Control),
            UseAlt = modifiers.HasFlag(KeyModifiers.Alt),
            UseShift = modifiers.HasFlag(KeyModifiers.Shift),
            UseWin = modifiers.HasFlag(KeyModifiers.Meta),
            KeyCode = GetVirtualKeyCode(key),
            KeyDisplayName = key.ToString()
        };

        TryApplyHotKey(config);
    }

    private static uint GetVirtualKeyCode(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
            return (uint)(key - Key.A + 0x41);
        if (key >= Key.D0 && key <= Key.D9)
            return (uint)(key - Key.D0 + 0x30);
        if (key >= Key.F1 && key <= Key.F12)
            return (uint)(key - Key.F1 + 0x70);

        return key switch
        {
            Key.Space => 0x20,
            Key.Return => 0x0D,
            Key.Tab => 0x09,
            Key.Back => 0x08,
            Key.Insert => 0x2D,
            Key.Delete => 0x2E,
            Key.Home => 0x24,
            Key.End => 0x23,
            Key.PageUp => 0x21,
            Key.PageDown => 0x22,
            Key.Up => 0x26,
            Key.Down => 0x28,
            Key.Left => 0x25,
            Key.Right => 0x27,
            Key.OemTilde => 0xC0,
            Key.OemMinus => 0xBD,
            Key.OemPlus => 0xBB,
            Key.OemOpenBrackets => 0xDB,
            Key.OemCloseBrackets => 0xDD,
            Key.OemSemicolon => 0xBA,
            Key.OemQuotes => 0xDE,
            Key.OemComma => 0xBC,
            Key.OemPeriod => 0xBE,
            Key.OemQuestion => 0xBF,
            Key.OemPipe => 0xDC,
            _ => 0
        };
    }
}
