# Geisterhand for Windows

Screen automation tool for Windows — HTTP API for remote control of applications via keyboard/mouse input, screenshots, UI automation, and menu discovery.

Wire-compatible with the macOS version so existing LLM prompts and clients work cross-platform.

## Quick Start

```bash
# Start the HTTP API server
geisterhand server

# Get system status
geisterhand status

# Launch an app with a scoped server
geisterhand run notepad

# Take a screenshot
geisterhand screenshot --output screen.png

# Type text
geisterhand type "Hello, World!"

# Press a key combination
geisterhand key c --modifiers ctrl

# Click at coordinates
geisterhand click 500 300

# Scroll
geisterhand scroll 500 300 --delta-y -3
```

## HTTP API

All endpoints are served on `http://127.0.0.1:7676` by default.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/status` | System status and running applications |
| GET | `/screenshot` | Capture screen or window screenshot |
| POST | `/click` | Click at screen coordinates |
| POST | `/type` | Type text |
| POST | `/key` | Press key with modifiers |
| POST | `/scroll` | Scroll at coordinates |
| POST | `/wait` | Wait for specified duration |
| GET | `/accessibility/tree` | Get UI element tree |
| GET | `/accessibility/search` | Search for UI elements |
| POST | `/accessibility/action` | Perform action on UI element |
| GET | `/menu/list` | List application menus |
| POST | `/menu/trigger` | Trigger a menu item |

### Examples

```bash
# Get status
curl http://127.0.0.1:7676/status

# Take screenshot of a specific app
curl "http://127.0.0.1:7676/screenshot?app_name=notepad"

# Type text
curl -X POST http://127.0.0.1:7676/type -H "Content-Type: application/json" \
  -d '{"text": "Hello, World!"}'

# Press Ctrl+S
curl -X POST http://127.0.0.1:7676/key -H "Content-Type: application/json" \
  -d '{"key": "s", "modifiers": ["ctrl"]}'

# Get accessibility tree
curl "http://127.0.0.1:7676/accessibility/tree?app_name=notepad&format=compact"

# Search for buttons
curl "http://127.0.0.1:7676/accessibility/search?app_name=notepad&role=AXButton"
```

## System Tray

Run `GeisterhandTray.exe` for a system tray icon that:
- Starts the HTTP server automatically
- Shows server status (green = running, gray = stopped)
- Provides start/stop/restart from the context menu

## Building from Source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build
dotnet test
```

### Publishing

```bash
# Self-contained single-file executables
dotnet publish src/Geisterhand.Cli -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish src/Geisterhand.Tray -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Architecture

- **Geisterhand.Core** — Shared library with all services:
  - Input: `KeyboardController`, `MouseController` (via Win32 `SendInput`/`PostMessage`)
  - Screen: `ScreenCaptureService` (via `BitBlt`/`PrintWindow`)
  - Accessibility: `AccessibilityService`, `MenuService` (via UI Automation)
  - Server: ASP.NET Core Minimal API with route handlers
- **Geisterhand.Cli** — Console app with System.CommandLine
- **Geisterhand.Tray** — WinForms system tray application

## Notes

- **Permissions:** Windows doesn't require macOS-style permission dialogs. Automation works by default. Running as administrator may be needed for interacting with elevated processes.
- **Modifier mapping:** `cmd`/`command` maps to the Windows key (`VK_LWIN`), `option` maps to `Alt`.
- **Bundle identifiers:** Windows doesn't have macOS bundle IDs; the `bundle_identifier` field returns the executable path.
