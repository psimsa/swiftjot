using Avalonia;
using System;
using System.Threading;

namespace SwiftJot;

sealed class Program
{
    private const string MutexName = "SwiftJot.SingleInstance";
    private const string EventName = "SwiftJot.ShowWindow";

    internal static Action? ShowMainWindow { get; set; }

    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            // Signal the already-running instance to show its window
            try
            {
                using var showEvent = EventWaitHandle.OpenExisting(EventName);
                showEvent.Set();
            }
            catch
            {
                // Running instance may not have set up the event yet; ignore
            }
            return;
        }

        using var waitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);

        // Listen for show-window signals from subsequent launch attempts
        var listenerThread = new Thread(() =>
        {
            while (true)
            {
                waitEvent.WaitOne();
                ShowMainWindow?.Invoke();
            }
        }) { IsBackground = true };
        listenerThread.Start();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
