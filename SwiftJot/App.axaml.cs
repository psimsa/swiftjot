using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SwiftJot.Views;
using System;

namespace SwiftJot;

public partial class App : Application
{
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;
            _mainWindow.Show();

            Program.ShowMainWindow = () => Dispatcher.UIThread.Post(ShowWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        ToggleWindow();
    }

    private void ShowMenuItem_Click(object? sender, EventArgs e)
    {
        ShowWindow();
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow?.Close(true);
            desktop.Shutdown();
        }
    }

    private void ToggleWindow()
    {
        if (_mainWindow is null) return;
        if (_mainWindow.IsVisible)
            _mainWindow.Hide();
        else
            ShowWindow();
    }

    internal void ShowWindow()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        _mainWindow.Activate();
        _mainWindow.BringIntoView();
    }
}
