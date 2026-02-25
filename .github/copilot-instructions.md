---
description: Primary AI agent instructions for SwiftJot development—Windows System Tray scratchpad with Avalonia UI and .NET Native AOT
applyTo: '**'
---

# SwiftJot: AI Agent Instructions

SwiftJot is a Windows System Tray scratchpad application built with C# and Avalonia UI, compiled with .NET Native AOT for instant startup and minimal memory footprint.

## Code Style

- **Philosophy:** Clean, lightweight code with minimal overengineering. Avoid unnecessary abstractions.
- **Language:** C# following Microsoft naming conventions (PascalCase for public members, _camelCase for private fields)
- **Formatting:** Standard .NET conventions; use `dotnet format` to ensure consistency
- **Comments:** Write self-documenting code; only comment the **why**, not the **what** (see task-specific instructions for code-commenting)
- **C# Guidelines:** Follow task-specific instructions for C# development

## Architecture & Core Patterns

### Native AOT Compatibility

**Every implementation must account for Native AOT constraints**—no runtime reflection, careful with heavy third-party libraries.

**Compiled Bindings (Avalonia):**
- Add `x:CompileBindings="True"` to every `.axaml` file
- The Avalonia compiler wires UI to ViewModels at build-time
- Avoids reflection crashes when published as AOT

**JSON Serialization:**
- Use C# Source Generators via `System.Text.Json`
- Define `JsonSerializable` attributes and partial `JsonSerializerContext` classes
- Example: `[JsonSerializable(typeof(List<Note>))] internal partial class AppJsonSerializerContext : JsonSerializerContext { }`

**Win32 Interop:**
- Use `[LibraryImport("user32.dll")]` attribute instead of `DllImport`
- Modern, AOT-friendly way to call native Windows APIs (e.g., `RegisterHotKey`, `UnregisterHotKey`)
- Integrate with Avalonia's window message loop to detect `WM_HOTKEY` (0x0312)

### Component Structure

- **ViewModels:** Located in `ViewModels/`, inherit from a simple base class with `INotifyPropertyChanged`
- **Views:** `.axaml` files in `Views/`, always with `x:CompileBindings="True"`
- **Models:** Data classes in `Models/` (e.g., `Note` with `Id`, `Title`, `Content`)
- **Services:** Business logic in `Services/` (e.g., `StorageService` for file I/O, `HotKeyService` for global shortcuts)
- **System Tray:** Configured in `App.axaml` via `<TrayIcon>` element

### Key Implementation Details

**Persistence:**
- Debounced saves (500ms after user stops typing) to avoid excessive file I/O
- Notes stored as JSON in `Environment.SpecialFolder.ApplicationData/SwiftJot/notes.json`
- Auto-load on startup; no "Save" button in UI

**Global Hotkey (e.g., Ctrl+Alt+Space):**
- Registered via Win32 `RegisterHotKey` during app startup
- Deregistered on app shutdown
- Triggers window show/activate + focus TextBox for immediate typing

**System Tray:**
- Single tray icon; click toggles window visibility
- Context menu with "Show" and "Exit" options
- Main window set to `ShowInTaskbar="False"` and `WindowStartupLocation="Manual"`
- On window close, hide instead of exit (preserve app in memory)

## Build & Test

```bash
# Restore dependencies
dotnet restore

# Build for debug (local testing)
dotnet build

# Build and test AOT compatibility (mandatory before commits)
dotnet publish -c Release -r win-x64

# Format code
dotnet format

# Run application locally (debug)
dotnet run
```

For AOT builds, test the resulting `.exe` in `bin/Release/net8.0-windows/win-x64/publish/` directly—it should launch sub-second.

## Project Conventions

- **Phases:** Development follows a phased approach (Foundation → System Tray → Persistence → Hotkeys → Export → Distribution). See [implementation-plan.md](../docs/implementation-plan.md) for details.
- **Tab Model:** The vertical tab interface supports multiple "jots"; users switch between them instantly.
- **Export:** Right-click context menu allows exporting current note to `.txt` or `.md` via native Windows file picker.
- **Startup Registration:** Optional helper to add app to Windows Startup folder for auto-launch on boot.
- **Light/Dark Mode:** Avalonia's Fluent theme responds to Windows system settings automatically.

## Integration Points

- **Avalonia Framework:** UI framework with built-in System Tray support and Fluent styling
- **System.Text.Json:** AOT-safe JSON serialization via Source Generators
- **Win32 API (user32.dll):** For global hotkey registration and window message interception
- **Windows AppData:** Single source of truth for persisted notes

## Security & Sensitive Areas

- **Win32 Interop:** Hotkey registration requires IntPtr and unmanaged code; test thoroughly on different Windows versions
- **File Permissions:** AppData folder is user-restricted; no privilege escalation needed
- **No External Dependencies:** The app intentionally avoids heavy NuGet packages that conflict with AOT

## Testing Strategy

1. **Phase Milestones:** At the end of each phase, publish with AOT and verify the `.exe` runs without reflection errors
2. **Hotkey Testing:** Run the app alongside other applications; verify hotkey works even when other apps have focus
3. **Persistence Testing:** Kill the app suddenly (Task Manager), restart, verify notes are intact
4. **Memory/Startup:** Monitor memory footprint and cold-start time after AOT publish; target sub-100MB resident set and sub-1s startup

## Getting Started

1. Familiarize yourself with the [specs](../docs/specs.md) and [implementation-plan](../docs/implementation-plan.md)
2. Check the current phase in [updates](../docs/updates.md)
3. Run `dotnet build` to ensure the environment is set up
4. Follow the phased approach; don't skip AOT testing—it's essential, not optional
