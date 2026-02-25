using System;
using System.Runtime.InteropServices;

namespace SwiftJot.Services;

public partial class HotKeyService : IDisposable
{
    private const int HotkeyId = 9000;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_ALT = 0x0001;
    private const uint VK_SPACE = 0x20;

    private IntPtr _hwnd;
    private bool _registered;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public bool Register(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _registered = RegisterHotKey(hwnd, HotkeyId, MOD_CONTROL | MOD_ALT, VK_SPACE);
        return _registered;
    }

    public bool IsHotKeyMessage(IntPtr wParam) => wParam.ToInt32() == HotkeyId;

    public void Dispose()
    {
        if (_registered)
            UnregisterHotKey(_hwnd, HotkeyId);
    }
}
