# Geisterhand Windows — Development Guide

## Build & Test

```bash
dotnet build                          # Build all projects
dotnet test                           # Run all tests
dotnet run --project src/Geisterhand.Cli -- status    # Run CLI
dotnet run --project src/Geisterhand.Cli -- server    # Start HTTP server
```

## Project Structure

- `src/Geisterhand.Core/` — Shared library: models, services, server, P/Invoke
- `src/Geisterhand.Cli/` — Console app (CLI) using System.CommandLine
- `src/Geisterhand.Tray/` — WinForms system tray app
- `tests/Geisterhand.Tests/` — xUnit tests

## Conventions

- **Target framework:** `net10.0-windows` (all projects)
- **JSON serialization:** `JsonNamingPolicy.SnakeCaseLower` — matches macOS API wire format
- **API models:** C# records in `Models/ApiModels.cs` with explicit `[JsonPropertyName]` attributes
- **P/Invoke:** Use `LibraryImport` (source-generated) in `Native/` classes, marked `internal static partial`
- **Accessibility:** System.Windows.Automation (UIA), role names mapped to AX-style strings via `RoleMap`
- **HTTP server:** ASP.NET Core Minimal API on port 7676
- **CLI:** System.CommandLine 2.0.0-beta5 — uses `Add()`, `SetAction()`, `ParseResult.GetValue()`

## Key Differences from macOS

- No permission dialogs needed (accessibility/screen recording work by default)
- `cmd` modifier → Windows key (`VK_LWIN`)
- `bundleIdentifier` → executable path (Windows has no bundle IDs)
- UIA ControlType → AX role strings mapped in `Accessibility/RoleMap.cs`

## Publishing

```bash
dotnet publish src/Geisterhand.Cli -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish src/Geisterhand.Tray -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```
