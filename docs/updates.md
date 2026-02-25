# SwiftJot: Distribution & Auto-Update Strategy

Thanks to .NET Native AOT, SwiftJot compiles down to a single, highly optimized native executable (usually under 20MB). This opens up two distinct distribution paths that cater to different types of users: a portable "binary copy" and a lightweight installer with seamless GitHub auto-updating.

*Note: This strategy is executed during **Phase 6** of the SwiftJot Implementation Plan.*

## Path 1: The Portable "Binary Copy" (No Install)

Because Native AOT statically links the .NET runtime and Avalonia UI into the executable, you do not need to ship an installer at all.

### How it Works

1.  Run `dotnet publish -c Release -r win-x64 /p:PublishAot=true`.
    
2.  Grab the resulting `SwiftJot.exe`.
    
3.  Distribute this single file directly to users (via GitHub Releases or a simple download link).
    

### Pros & Cons

*   **Pros:** True zero-friction usage. Users can drop it anywhere (even a USB drive) and double-click to run. No admin rights are required.
    
*   **Cons:** The user has to manually place it in their `shell:startup` folder if they want it to run on boot. They also have to manually download new versions.
    

## Path 2: Lightweight Installer & Auto-Update (Using Velopack)

To achieve a seamless, modern application experience with auto-updates pulling directly from GitHub, **Velopack** is the industry standard for modern Avalonia and AOT applications. It is the Rust-based spiritual successor to Squirrel.Windows.

### Why Velopack?

*   **AOT Compatible:** Unlike older update frameworks that heavily rely on reflection, Velopack is highly compatible with Native AOT trimming.
    
*   **Zero-Click Installer:** It generates a highly compressed `Setup.exe` that installs the app in about 2 seconds without annoying Next/Next/Finish wizards.
    
*   **Native GitHub Support:** It natively reads your public or private GitHub repository's release tags and assets.
    
*   **Delta Updates:** It generates "Delta" packages, meaning if you change a few lines of code, the auto-update download might only be a few kilobytes instead of the full 20MB executable.
    

### The Developer Workflow (CI/CD)

Whenever you are ready to release a new version of SwiftJot, you will use the Velopack CLI (`vpk`). Ideally, this should be automated using a **GitHub Actions** workflow triggered on a new tag (e.g., `v1.0.0`):

1.  **Publish:** `dotnet publish -c Release -r win-x64 /p:PublishAot=true`
    
2.  **Pack:** `vpk pack -u SwiftJot -v 1.0.1 -p path/to/publish/dir -e SwiftJot.exe`
    
3.  **Release:** Upload the generated `-Setup.exe`, `.nupkg`, and `RELEASES` files directly to a new GitHub Release.
    

### The Application Code (C# Integration)

Because SwiftJot runs silently in the system tray, updates should be invisible and non-intrusive.

You will integrate the `Velopack` NuGet package into your Avalonia `App.axaml.cs` startup logic:

```
using Velopack;

public static async Task CheckForUpdates()
{
    // Point Velopack directly to your GitHub repository
    var mgr = new UpdateManager(new GithubSource("[https://github.com/YourUsername/SwiftJot](https://github.com/YourUsername/SwiftJot)", string.Empty, false));

    // Check for new releases on GitHub
    var updateInfo = await mgr.CheckForUpdatesAsync();
    
    if (updateInfo != null)
    {
        // Silently download the update in the background
        await mgr.DownloadUpdatesAsync(updateInfo);
        
        // At this point, the update is ready. 
        // We can either apply it immediately and restart, 
        // OR wait for the user to reboot their PC natively.
        
        // Example: Apply & Restart
        // mgr.ApplyUpdatesAndRestart(updateInfo);
    }
}
```

### UX Integration for System Tray Updates

Since SwiftJot doesn't have a traditional main window visible all the time, the update flow should be tied to the Tray Icon:

1.  App checks for updates silently on startup (or every 24 hours).
    
2.  If an update is downloaded, change the Avalonia `<TrayIcon>` to a different graphic (e.g., adding a small green dot).
    
3.  Add a right-click context menu item to the tray: `"Update Ready: Restart SwiftJot"`.
    
4.  If the user clicks it, call `mgr.ApplyUpdatesAndRestart(updateInfo)`. If they ignore it, the update automatically applies the next time they turn on their computer.
    

## Summary Recommendation

Offer **both**.

Configure your GitHub Actions (or local build script) to output a zipped portable `SwiftJot.exe` (Path 1) alongside the Velopack `SwiftJot-Setup.exe` (Path 2) on your GitHub Releases page. This caters to power users who want standalone binaries and standard users who want frictionless auto-updates.
