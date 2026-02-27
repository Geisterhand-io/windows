# Geisterhand Windows — Development Guide

## Build & Test

```bash
dotnet build                                           # Build all projects
dotnet test                                            # Run all tests (58 xUnit tests)
dotnet run --project src/Geisterhand.Cli -- --help     # Show CLI help
dotnet run --project src/Geisterhand.Cli -- status     # Run a CLI command
dotnet run --project src/Geisterhand.Cli -- server     # Start HTTP server on :7676
```

## Project Structure

```
Geisterhand.sln
├── src/
│   ├── Geisterhand.Core/           # Shared library — all services, models, server
│   │   ├── Models/
│   │   │   ├── ApiModels.cs         # All request/response records (1:1 macOS parity)
│   │   │   └── AccessibilityModels.cs  # Internal WindowInfo, ElementInfo
│   │   ├── Input/
│   │   │   ├── KeyboardController.cs  # SendInput + PostMessage keyboard input
│   │   │   ├── MouseController.cs     # SendInput + PostMessage mouse input
│   │   │   └── KeyCodeMap.cs          # Key name → VK_ code + modifier mapping
│   │   ├── Accessibility/
│   │   │   ├── AccessibilityService.cs  # UIA tree traversal, search, actions
│   │   │   ├── MenuService.cs           # Menu bar discovery + trigger via UIA
│   │   │   └── RoleMap.cs              # UIA ControlType → AX role string mapping
│   │   ├── Screen/
│   │   │   ├── ScreenCaptureService.cs  # BitBlt + PrintWindow + window discovery
│   │   │   └── ImageEncoder.cs          # PNG/JPEG/Base64 encoding
│   │   ├── Permissions/
│   │   │   └── PermissionManager.cs     # Admin detection (always-true for AX/screen)
│   │   ├── Server/
│   │   │   ├── GeisterhandServer.cs     # ASP.NET Core Minimal API + JSON config
│   │   │   ├── ServerManager.cs         # Start/stop/restart lifecycle + port finder
│   │   │   └── Routes/
│   │   │       ├── StatusRoute.cs       # GET /status
│   │   │       ├── ScreenshotRoute.cs   # GET /screenshot
│   │   │       ├── ClickRoute.cs        # POST /click
│   │   │       ├── TypeRoute.cs         # POST /type
│   │   │       ├── KeyRoute.cs          # POST /key
│   │   │       ├── ScrollRoute.cs       # POST /scroll
│   │   │       ├── WaitRoute.cs         # POST /wait
│   │   │       ├── AccessibilityRoute.cs  # GET/POST /accessibility/*
│   │   │       └── MenuRoute.cs         # GET/POST /menu/*
│   │   └── Native/
│   │       ├── User32.cs              # P/Invoke: SendInput, window/cursor APIs
│   │       ├── Kernel32.cs            # P/Invoke: process APIs
│   │       └── Gdi32.cs              # P/Invoke: BitBlt, GDI bitmap APIs
│   │
│   ├── Geisterhand.Cli/              # Console app — System.CommandLine CLI
│   │   ├── Program.cs                # Root command + 8 subcommands
│   │   └── Commands/
│   │       ├── StatusCommand.cs       # geisterhand status
│   │       ├── ScreenshotCommand.cs   # geisterhand screenshot
│   │       ├── ClickCommand.cs        # geisterhand click <x> <y>
│   │       ├── TypeCommand.cs         # geisterhand type <text>
│   │       ├── KeyCommand.cs          # geisterhand key <key>
│   │       ├── ScrollCommand.cs       # geisterhand scroll <x> <y>
│   │       ├── ServerCommand.cs       # geisterhand server
│   │       └── RunCommand.cs          # geisterhand run <app>
│   │
│   └── Geisterhand.Tray/             # WinForms system tray app
│       ├── Program.cs                # Entry point ([STAThread])
│       ├── TrayApplicationContext.cs  # NotifyIcon, context menu, auto-start
│       └── StatusMonitor.cs          # 2-second polling for server status
│
└── tests/
    └── Geisterhand.Tests/            # xUnit tests
        ├── Models/ApiModelsTests.cs   # JSON serialization round-trip tests
        └── Input/KeyCodeMapTests.cs   # Key mapping + modifier resolution tests
```

## Coding Conventions

### Framework & Tooling
- **Target framework:** `net10.0-windows` (all projects)
- **UI Automation access:** `<UseWPF>true</UseWPF>` in Core csproj (provides `System.Windows.Automation`)
- **P/Invoke:** `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` for `LibraryImport` source generation
- **CLI library:** System.CommandLine `2.0.0-beta5.25306.1`
- **Test framework:** xUnit `2.9.3` with `Microsoft.NET.Test.Sdk 17.13.0`

### JSON Serialization
- **Naming policy:** `JsonNamingPolicy.SnakeCaseLower` — wire-compatible with macOS Swift `convertToSnakeCase`
- **Null handling:** `JsonIgnoreCondition.WhenWritingNull` — null fields omitted from output
- **Case-insensitive deserialization:** `PropertyNameCaseInsensitive = true`
- **Model pattern:** C# `record` types with explicit `[JsonPropertyName("snake_case")]` attributes
- Canonical JSON options live on `GeisterhandServer.JsonOptions` — always use that, never create new options

### P/Invoke
- Use `LibraryImport` (source-generated) in `Native/` classes, marked `internal static partial`
- Exception: `GetWindowTextW` uses classic `DllImport` with `char[]` buffer (the `LibraryImport` `out string` marshalling causes AccessViolation)
- Group by DLL: `User32.cs`, `Kernel32.cs`, `Gdi32.cs`
- All constants (WM_*, VK_*, MOUSEEVENTF_*, etc.) live alongside their P/Invoke declarations

### System.CommandLine beta5 API
The 2025 beta5 API is different from older tutorials. Key patterns:
```csharp
// Options: constructor takes name + optional aliases only. Set Description/DefaultValueFactory as properties.
var opt = new Option<int>("--port") { Description = "...", DefaultValueFactory = _ => 7676 };

// Arguments: constructor takes name only.
var arg = new Argument<string>("app") { Description = "..." };

// Adding to command: always use Add(), not AddCommand/AddOption/AddArgument.
command.Add(opt);
command.Add(arg);

// Handlers: use SetAction(), not SetHandler(). Get values from ParseResult.
command.SetAction((parseResult) => {
    int port = parseResult.GetValue(opt);
});
// Async variant:
command.SetAction(async (parseResult, ct) => { ... });

// Invocation: use CommandLineConfiguration, not RootCommand.InvokeAsync().
var config = new CommandLineConfiguration(rootCommand);
return await config.InvokeAsync(args);
```

### Scoped Servers
The `run` command creates a `GeisterhandServer` with a `ServerContext(targetPid, targetAppName)`. When these are set, every route handler automatically uses them as defaults for `app_name`/`pid` — callers can omit those fields and the server auto-targets the launched app. This is the primary workflow for LLM tool-use: `geisterhand run notepad` → use the returned port → all requests implicitly target Notepad.

### Window Discovery for Store/UWP Apps
Modern Windows Store apps (like the new Notepad, Calculator) launch through a broker process that exits immediately. `RunCommand.LaunchAndFindProcess` handles this by:
1. Snapshotting all existing window PIDs before launch
2. Starting the app
3. Polling for any new window whose PID wasn't in the snapshot
4. Matching by process name or window title first, then accepting any new window after ~1.5s

## Key Differences from macOS

| macOS | Windows |
|-------|---------|
| AXUIElement + Accessibility API | System.Windows.Automation (UIA) |
| CGEvent for input | SendInput + PostMessage via P/Invoke |
| kVK_* key codes | VK_* virtual key codes |
| `cmd` modifier = Command key | `cmd` modifier = Windows key (VK_LWIN) |
| `option` modifier = Option key | `option` modifier = Alt key (VK_MENU) |
| Bundle identifiers (com.apple.TextEdit) | Executable paths (C:\...\notepad.exe) |
| Permission dialogs (accessibility, screen recording) | Always granted (may need admin for elevated apps) |
| Hummingbird HTTP server | ASP.NET Core Minimal API |
| ScreenCaptureKit / CGWindowListCreateImage | BitBlt + PrintWindow |
| kAXRoleAttribute string | UIA ControlType → mapped to same AX string via RoleMap |
| `CGEvent.postToPid()` | PostMessage(hwnd, WM_*) |

## Publishing

```bash
# Self-contained single-file executables for distribution
dotnet publish src/Geisterhand.Cli -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish src/Geisterhand.Tray -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Output: `geisterhand.exe` (CLI) and `GeisterhandTray.exe` (tray app).

## CI/CD

- `.github/workflows/ci.yml` — Build + test on push/PR to `main`
- `.github/workflows/release.yml` — On `v*` tags: build, test, publish, create GitHub Release with ZIP
