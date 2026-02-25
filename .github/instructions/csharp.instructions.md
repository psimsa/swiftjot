---
description: C# development guidelines including naming conventions, formatting, nullable reference types, XML documentation, and AOT compatibility
applyTo: '**/*.cs'
---

# C# Development Guidelines

## Language & Version
- Use C# 14 features and latest language capabilities
- Follow self-documenting code principles—prioritize clarity over commentary

## Naming Conventions

- **PascalCase:** Component names, method names, public members, properties
- **camelCase:** Private fields and local variables
- **Interface Prefix:** Interface names start with "I" (e.g., `IStorageService`)
- Use `nameof()` instead of string literals when referring to member names

## Formatting

- Follow `.editorconfig` style definitions
- Use file-scoped namespace declarations: `namespace SwiftJot.Services;`
- Insert newline before opening curly brace of code blocks
- Final return statements on their own line
- Use pattern matching and switch expressions wherever possible
- Ensure XML doc comments exist for all public APIs

## Nullable Reference Types

- Declare variables non-nullable; check for `null` at entry points
- Use `is null` or `is not null` instead of `== null` or `!= null`
- Trust C# null annotations—don't add null checks when the type system guarantees non-null

## XML Documentation

For public APIs, use concise XML documentation:

```csharp
/// <summary>
/// Saves the provided notes to persistent storage synchronously, debounced to avoid excessive I/O.
/// </summary>
/// <param name="notes">The list of notes to persist.</param>
/// <exception cref="IOException">Thrown if the file write fails.</exception>
public void SaveNotes(List<Note> notes)
{
    // ... implementation
}
```

For single-line methods, compress:

```csharp
/// <summary>Gets the path to the application data directory for SwiftJot.</summary>
public string GetAppDataPath() => Path.Combine(AppDataPath, "SwiftJot");
```

## Error Handling

- Handle exceptions at appropriate boundaries (entry points for external calls, service methods)
- Use specific exception types rather than catching generic `Exception`
- Provide meaningful error context in exception messages
- Document exceptions in XML comments with `<exception>` tags

## Code Organization

- Group related methods together logically
- Place public members before private members
- Extract large methods into focused, single-responsibility methods
- Use `private` by default; expose only what's necessary

## Performance Considerations

- Mark I/O-bound operations as `async` (e.g., file reading, JSON serialization)
- Use `Task` and `Task<T>` for asynchronous operations
- Implement debounced operations for frequent events (e.g., text changed → save)
- Profile before optimizing; prefer clarity unless a clear performance issue exists

## AOT Compatibility in C#

- Avoid reflection; use compile-time approaches (attributes, source generators, static analysis)
- Use `[JsonSerializable]` and `JsonSerializerContext` for AOT-safe JSON serialization
- Prefer `[LibraryImport]` over `DllImport` for Win32 interop
- Test every build with `dotnet publish -c Release -r win-x64` to catch AOT issues early
