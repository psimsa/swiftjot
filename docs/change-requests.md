# SwiftJot: Change Requests & Enhancements

This document outlines change requests and enhancements that diverge from or extend the original specifications in [specs.md](specs.md) and [updates.md](updates.md).

## 1. Custom Hotkey Configuration UI

### Current State
The global hotkey is hardcoded as `Ctrl + Alt + Space` to summon the application and create a new note.

### Change Request
Implement a UI within SwiftJot to allow users to define and customize the global hotkey for creating a new note.

### Implementation Details

- **UI Location:** Add a "Settings" or "Preferences" option in the tray icon context menu or as a dedicated settings tab in the main window.
- **Hotkey Picker:** Provide an interactive control where users can click and press the desired key combination (e.g., "Press your desired hotkey...").
- **Validation:** Ensure the hotkey is not already registered by another application; display a warning if conflicts are detected.
- **Persistence:** Save the custom hotkey to the `AppData` configuration file alongside the notes JSON.
- **Runtime Registration:** On app startup, unregister the old hotkey and register the newly configured one.

### Benefits
- Users can avoid conflicts with other applications' hotkeys.
- Accessibility: Users with specific keyboard layouts may prefer alternative shortcuts.

---

## 2. Auto-Generated Note Titles from First Line

### Current State
Notes must have a user-defined title or name.

### Change Request
If a user creates a new note without specifying a title, the application should automatically use the first line of the note content as the title.

### Implementation Details

- **Title Generation Logic:**
  - When a new note is created, the title field is empty by default.
  - As the user types the first line, capture this text and auto-populate the title field.
  - Trim whitespace and limit the title to a reasonable length (e.g., first 50 characters).
  - If the user manually enters a title later, use the manual title instead.

- **Note Model Update:**
  - The `Note` class remains unchanged; the title is still a required property in storage.
  - Generation occurs at the UI/ViewModel level, not in the persistence layer.

- **Behavior:**
  - Auto-generation only occurs for notes that don't have a user-specified title.
  - Once a title is manually set, the auto-generation logic ceases for that note.
  - Editing the first line of the content does not retroactively change the auto-generated title (to avoid confusion).

### Benefits
- Reduced friction: Users don't need to think about naming quick notes.
- Maintains context: The title reflects the note's content naturally.

---

## 3. Single Application Instance (Singleton)

### Current State
Multiple instances of SwiftJot can be launched simultaneously, which may cause data conflicts and confusing behavior.

### Change Request
Enforce that only one instance of SwiftJot may run at a time. If a user attempts to launch a second instance, the existing instance should be brought to the foreground instead.

### Implementation Details

- **Mutex-Based Locking:**
  - Use a named `Mutex` (e.g., `"SwiftJot.SingleInstance"`) in the application startup logic (`Program.cs` or `App.axaml.cs`).
  - Attempt to acquire the mutex during app initialization.
  - If the mutex is already held, another instance is running.

- **Behavior When Second Instance is Launched:**
  - Detect the running instance via a named pipe, local message queue, or similar IPC mechanism.
  - Send a signal to the running instance to activate its main window.
  - Terminate the new instance gracefully.

- **Integration Points:**
  - Implement in `Program.cs` before the Avalonia app is created.
  - Alternatively, implement in `App.xaml.cs` initialization.

### Benefits
- Prevents data corruption from concurrent writes to the notes JSON file.
- Provides expected user behavior: launching the app again focuses the running instance.
- Simplifies the storage and update mechanisms.

---

## 4. Exit Application Option & Window Close Behavior Configuration

### Current State
- Closing the main window (clicking the X button) hides the window and keeps the app running in the system tray.
- There is no built-in UI option to completely exit the application.
- The only way to exit is via the tray icon context menu's "Exit" option.

### Change Request
Implement the following enhancements:

1. **UI Option to Exit Completely:**
   - Add a visible "Exit Application" button or menu option within the main window UI (not just in the tray context menu).
   - This option should completely terminate the application.

2. **X Button Behavior Configuration:**
   - Add a configuration option (accessible via Settings/Preferences) to define the default behavior of the window's X button.
   - Options:
     - **"Minimize to Tray"** (default): The current behavior—close button hides the window.
     - **"Exit Application"**: The close button completely terminates the application.
   - Save the user's preference to the configuration file.

### Implementation Details

- **Settings UI:**
  - Add a "Behavior" or "Preferences" section in the main window or a dedicated settings tab.
  - Include a toggle or radio button group: "When closing the window: [Minimize to Tray] / [Exit Application]".

- **Window Close Logic:**
  - In `MainWindow.axaml.cs`, override the `OnClosing` event.
  - Read the user's preference from the configuration.
  - If "Minimize to Tray": call `Hide()` (current behavior).
  - If "Exit Application": call `Application.Current.Shutdown()` or similar.

- **Config Persistence:**
  - Store the preference in a new configuration file (e.g., `config.json`) or extend the existing persistence mechanism.
  - Load the preference on app startup.

### Benefits
- Improved user agency: Users can choose their preferred interaction model.
- Accessibility: Power users who prefer keyboard shortcuts can quickly exit.
- Clarity: The "Exit Application" button makes it obvious that the app can be completely terminated, not just hidden.

---

## 5. Inline Delete Button for Notes

### Current State
Notes are removed via a dedicated "-" (minus) button at the bottom of the vertical tab list.

### Change Request
Add an inline delete button ("x") to each note tab, aligned to the right side of the tab. This provides a more intuitive and direct way to remove individual notes without needing to select the note and use a separate control.

### UI Layout
```
| Jot 1                      x |
| My sample note    x |
| Foo                        x |
```

### Implementation Details

- **Tab Template Modification:**
  - Modify the `TabControl` template in `MainWindow.axaml` to include an inline button within each tab header.
  - Use a `StackPanel` with `Orientation="Horizontal"` and `HorizontalAlignment="Stretch"` to contain the tab title and delete button.
  - Align the delete button to the right using `HorizontalAlignment="Right"` or similar.

- **Delete Button Styling:**
  - Style the "x" button to be subtle and small (e.g., 14-16px font, transparent background).
  - Use a hover effect to highlight the button when the user hovers over a tab (improves discoverability).
  - Ensure the button is accessible and clearly clickable.

- **Click Behavior:**
  - Bind the button's `Click` event to a command in the ViewModel that removes the selected note.
  - Confirm with the user before deletion (optional: add a confirmation dialog or toast notification).
  - After deletion, focus on another available tab (e.g., the previous or next note).

- **Keyboard Navigation:**
  - Ensure the delete button is reachable via Tab key for accessibility.
  - Optionally, support a keyboard shortcut (e.g., `Delete` key while a tab is focused) to delete the current note.

### Benefits
- **Improved UX:** Users can delete notes directly without manual selection and separate actions.
- **Discoverability:** The inline button is more visible than a separate control at the bottom.
- **Consistency:** Inline delete buttons are a familiar pattern (e.g., browser tabs, email clients).

---

## 5. Lightweight Installer Not Yet Implemented

### Current State
The [updates.md](updates.md) document describes **Path 2: Lightweight Installer & Auto-Update (Using Velopack)** as a distribution option that would be implemented in **Phase 6** of the development plan.

### Change Request Status
**This feature is not yet implemented.** The current release workflow ([.github/workflows/release.yml](.github/workflows/release.yml)) generates portable zip archives (Path 1) for both `win-x64` and `win-arm64` architectures but does not generate Velopack installers or auto-update infrastructure.

### Current Release Process
1. Build for both architectures: `dotnet publish -c Release -r win-x64` and `dotnet publish -c Release -r win-arm64`.
2. Package the publish directories into zip files.
3. Upload zip files to GitHub Releases.

### Future Implementation Notes
When Velopack integration is implemented, the following will be added:

- Install the `Velopack` NuGet package to the SwiftJot project.
- Implement `UpdateManager` logic in `App.axaml.cs` to check for and apply updates from GitHub.
- Extend the GitHub Actions workflow to:
  - Run `vpk pack` for each architecture.
  - Generate `-Setup.exe` installers for both architectures.
  - Generate delta update packages (`.nupkg` files).
  - Upload all installer and update files to GitHub Releases.
- Tray icon integration to notify users of available updates.

### Recommendation
For now, users are directed to download the portable zip archives. A future release will provide seamless installer + auto-update support via Velopack.

---

## Summary of Changes

| Feature | Status | Priority |
|---------|--------|----------|
| Custom Hotkey Configuration UI | Requested | High |
| Auto-Generated Note Titles | Requested | Medium |
| Single Instance Enforcement | Requested | High |
| Exit Application Option | Requested | Medium |
| X Button Behavior Configuration | Requested | Medium |
| Inline Delete Button for Notes | Requested | Medium |
| Velopack Installer & Auto-Update | Deferred | Low (Future Phase 6) |

