using System;
using System.Runtime.InteropServices;
using SwiftJot.Models;

namespace SwiftJot.Services;

public partial class HotKeyService : IDisposable
{
    private const int HotkeyId = 9000;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_SHIFT = 0x0004;

    private IntPtr _hwnd;
    private bool _registered;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public bool Register(IntPtr hwnd, HotKeyConfig? config = null)
    {
        _hwnd = hwnd;
        var modifiers = config is null ? MOD_CONTROL | MOD_ALT : BuildModifiers(config);
        var keyCode = config?.KeyCode ?? 0x20;
        _registered = RegisterHotKey(hwnd, HotkeyId, modifiers, keyCode);
        return _registered;
    }

    public bool IsHotKeyMessage(IntPtr wParam) => wParam.ToInt32() == HotkeyId;

    public void Dispose()
    {
        if (_registered)
            UnregisterHotKey(_hwnd, HotkeyId);
    }

    private static uint BuildModifiers(HotKeyConfig config)
    {
        uint mods = 0;
        if (config.UseCtrl) mods |= MOD_CONTROL;
        if (config.UseAlt) mods |= MOD_ALT;
        if (config.UseShift) mods |= MOD_SHIFT;
        return mods;
    }
}
