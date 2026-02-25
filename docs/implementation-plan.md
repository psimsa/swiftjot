# SwiftJot: Phased Implementation Plan

This document outlines the step-by-step development process for building, testing, and distributing SwiftJot. Because we are targeting **Native AOT**, it is critical to test AOT compatibility at the end of every phase, rather than waiting until the end of the project.

## Phase 1: Foundation & Project Setup

**Goal:** Establish the Avalonia boilerplate and configure the project for Native AOT.

1.  **Initialize Project:**
    
    *   Run `dotnet new avalonia.app -n SwiftJot`.
        
    *   Update Avalonia NuGet packages to the latest stable version.
        
2.  **AOT Configuration:**
    
    *   Open `SwiftJot.csproj`.
        
    *   Add `<PublishAot>true</PublishAot>`.
        
    *   Add `<InvariantGlobalization>true</InvariantGlobalization>` (reduces file size if international globalization isn't strictly needed for core logic).
        
3.  **Enforce Compiled Bindings:**
    
    *   Add `x:CompileBindings="True"` to `MainWindow.axaml` and ensure the project builds.
        
4.  **Milestone Test:** \* Run `dotnet publish -c Release -r win-x64`. Ensure the resulting `.exe` launches successfully.
    

## Phase 2: System Tray & Core UI Layout

**Goal:** Make the app run in the background without a taskbar icon and build the basic layout.

1.  **Tray Icon Setup:**
    
    *   In `App.axaml`, add the `<TrayIcon.Icons>` definition.
        
    *   Configure a context menu for the tray icon (e.g., "Show", "Exit").
        
2.  **Window Configuration:**
    
    *   Set `ShowInTaskbar="False"` in `MainWindow.axaml`.
        
    *   Implement a method to toggle `MainWindow.IsVisible` when the tray icon is left-clicked.
        
    *   Override the window's `OnClosing` event to simply hide the window (`Hide()`) instead of destroying the application process.
        
3.  **Vertical Tabs Layout:**
    
    *   Add a `TabControl` to `MainWindow.axaml`.
        
    *   Set `TabStripPlacement="Left"`.
        
    *   Create a basic `TextBox` inside the `TabControl.ContentTemplate` to serve as the scratchpad.
        
4.  **Milestone Test:** \* Launch the app. Ensure it appears in the tray, hides/shows correctly, and does not show up in the Alt-Tab or Taskbar menus.
    

## Phase 3: Data Persistence (AOT-Safe)

**Goal:** Implement auto-saving functionality without using runtime reflection.

1.  **Data Models:**
    
    *   Create a `Note` class with `Id`, `Title`, and `Content` properties.
        
2.  **JSON Source Generators:**
    
    *   Create an AOT-safe serialization context:
        
        ```
        [JsonSerializable(typeof(List<Note>))]
        internal partial class AppJsonSerializerContext : JsonSerializerContext { }
        ```
        
3.  **File I/O Logic:**
    
    *   Write a simple `StorageService` that saves to `Environment.SpecialFolder.ApplicationData + "/SwiftJot/notes.json"`.
        
    *   Hook up a `TextChanged` event (or Rx equivalent in Avalonia) on the `TextBox` to trigger a debounced save operation (e.g., save 500ms after the user stops typing).
        
4.  **Milestone Test:** \* Type text, forcefully kill the app via Task Manager, and restart it. Verify the text is restored perfectly. Publish via AOT and verify serialization doesn't crash.
    

## Phase 4: Global Hotkey Integration

**Goal:** Allow the user to summon the app instantly from anywhere in Windows.

1.  **Win32 Interop:**
    
    *   Define AOT-safe native methods using `LibraryImport`:
        
        ```
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        ```
        
2.  **Window Message Hooking:**
    
    *   In Avalonia, access the underlying Win32 window handle (`HWND`) of `MainWindow`.
        
    *   Hook into the Avalonia window's `WndProc` (or use `IWindowImpl`) to listen for the `WM_HOTKEY` (0x0312) message.
        
3.  **Action Routing:**
    
    *   When the hotkey is detected, call `MainWindow.Show()` and `MainWindow.Activate()`. Ensure the `TextBox` requests focus immediately so the user can just start typing.
        
4.  **Milestone Test:** \* Minimize the app, open Google Chrome, and press the global hotkey. The app should pop up over Chrome, ready for text input.
    

## Phase 5: Exporting & Polish

**Goal:** Add secondary features and refine the user experience.

1.  **Export Functionality:**
    
    *   Add an "Export to TXT" button in the UI.
        
    *   Use Avalonia's `StorageProvider` to show a native Windows "Save As" dialog and write the current tab's text to the selected path.
        
2.  **Startup Registration:**
    
    *   Write a helper method that adds a shortcut to `Environment.SpecialFolder.Startup` so the app launches silently on boot.
        
3.  **Theming:**
    
    *   Ensure Avalonia's Fluent theme responds correctly to Windows Light/Dark mode changes.
        
4.  **Milestone Test:** \* Fully publish the app natively. Install it on a fresh Windows VM, enable "Run on Startup", reboot the VM, and test the entire workflow.
    

## Phase 6: Velopack & CI/CD Distribution

**Goal:** Package the app for end-users with seamless GitHub auto-updating.

1.  **Velopack Integration:**
    
    *   Install the `Velopack` NuGet package.
        
    *   Implement the `UpdateManager` logic in `App.axaml.cs` (as outlined in the Distribution Strategy document).
        
    *   Tie the update notification to a UI change in the Tray Icon context menu.
        
2.  **GitHub Actions Pipeline:**
    
    *   Create `.github/workflows/release.yml`.
        
    *   Set up a trigger for pushed tags (e.g., `v*`).
        
    *   Configure steps to:
        
        1.  Setup .NET and Rust (for Velopack).
            
        2.  Run `dotnet publish` with AOT flags.
            
        3.  Run `vpk pack`.
            
        4.  Upload the generated `-Setup.exe` and `.nupkg` assets to the GitHub Release.
            
3.  **Milestone Test:** \* Push a `v1.0.0` tag. Download the built installer from GitHub, install it. Make a minor text change, push `v1.0.1`, and verify the installed app detects, downloads, and applies the update silently.
