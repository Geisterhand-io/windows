# Geisterhand for Windows

Screen automation tool for Windows. Provides an HTTP API for remote control of applications via keyboard/mouse input, screenshots, UI automation, and menu discovery.

Wire-compatible with the [macOS version](https://github.com/nickthedude/geisterhand) — same endpoints, same JSON format, same AX-style role names — so existing LLM prompts and clients work cross-platform.

## Install

### From GitHub Releases

Download the latest `geisterhand-windows-x64.zip` from [Releases](../../releases), extract, and add to your PATH.

### From Source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet publish src/Geisterhand.Cli -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
# Binary at publish/geisterhand.exe
```

---

## Quick Start

The most common workflow — launch an app and get a scoped API server for it:

```bash
$ geisterhand run notepad
{"app_name":"Notepad","pid":12345,"port":7677,"base_url":"http://127.0.0.1:7677"}
```

All subsequent requests to port 7677 automatically target that Notepad instance — no need to pass `app_name` or `pid`:

```bash
curl -X POST http://127.0.0.1:7677/type -d '{"text":"Hello from Geisterhand"}'
curl http://127.0.0.1:7677/screenshot
curl http://127.0.0.1:7677/accessibility/tree?format=compact
```

Or start a global server that can target any app by name:

```bash
geisterhand server                    # Starts on http://127.0.0.1:7676
curl http://127.0.0.1:7676/status     # List all running apps
curl "http://127.0.0.1:7676/screenshot?app_name=notepad"
```

---

## CLI Reference

```
geisterhand [command] [options]
```

### `status`

Show system status and running applications.

```bash
geisterhand status [--port 7676]
```

Tries to connect to a running server first. If no server is running, generates status locally.

### `server`

Start the HTTP API server.

```bash
geisterhand server [--port 7676]
```

Runs until Ctrl+C.

### `run <app>`

Launch an application and start a scoped server for it.

```bash
geisterhand run <app> [--port <port>]
```

- `<app>` — process name (e.g. `notepad`), window title, or full executable path
- `--port` — server port (auto-assigned starting from 7677 if omitted)

Outputs a JSON line with `app_name`, `pid`, `port`, and `base_url`. The server stays running until the app exits or you press Ctrl+C. All API requests to the scoped server automatically target the launched app.

Handles modern Windows Store apps (Notepad, Calculator) where the initial broker process exits and respawns as a different PID.

### `screenshot`

Take a screenshot.

```bash
geisterhand screenshot [--app-name <name>] [--pid <pid>] [--format png|jpeg] [--quality 85] [--output <path>]
```

Without `--output`, prints base64-encoded JSON. With `--output`, saves the image file directly.

### `click <x> <y>`

Click at screen coordinates.

```bash
geisterhand click <x> <y> [--button left|right|middle] [--click-type single|double|triple] [--app-name <name>] [--pid <pid>]
```

### `type <text>`

Type text into the focused application.

```bash
geisterhand type <text> [--app-name <name>] [--pid <pid>]
```

### `key <key>`

Press a key with optional modifiers.

```bash
geisterhand key <key> [--modifiers <mod1> <mod2> ...] [--app-name <name>] [--pid <pid>]
```

Examples:
```bash
geisterhand key return                    # Press Enter
geisterhand key c --modifiers ctrl        # Ctrl+C
geisterhand key s --modifiers ctrl shift  # Ctrl+Shift+S
geisterhand key tab --modifiers alt       # Alt+Tab
```

### `scroll <x> <y>`

Scroll at screen coordinates.

```bash
geisterhand scroll <x> <y> [--delta-x <n>] [--delta-y <n>] [--app-name <name>] [--pid <pid>]
```

Positive `delta-y` scrolls up, negative scrolls down.

---

## HTTP API Reference

Base URL: `http://127.0.0.1:7676` (global server) or the `base_url` from `geisterhand run`.

All POST endpoints accept `Content-Type: application/json`. All responses are JSON with `snake_case` field names. Null fields are omitted.

### `GET /status`

Returns system status and all visible applications.

**Response:**
```json
{
  "status": "ok",
  "version": "1.0.0",
  "platform": "windows",
  "api_version": "1",
  "permissions": {
    "accessibility": true,
    "screen_recording": true
  },
  "running_applications": [
    {
      "name": "Notepad",
      "bundle_identifier": "C:\\...\\Notepad.exe",
      "pid": 12345,
      "is_active": true
    }
  ]
}
```

`bundle_identifier` is the executable path on Windows (macOS returns the app bundle ID).

---

### `GET /screenshot`

Capture a screenshot as a base64-encoded image.

**Query parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `app_name` | string | _(server target)_ | Application name or window title |
| `pid` | int | _(server target)_ | Process ID |
| `format` | string | `"png"` | Image format: `png` or `jpeg` |
| `quality` | int | `85` | JPEG quality (1-100, ignored for PNG) |

If neither `app_name` nor `pid` is given (and no server target), captures the full screen.

**Response:**
```json
{
  "image": "iVBORw0KGgo...",
  "format": "png",
  "width": 1920,
  "height": 1080
}
```

---

### `POST /click`

Click at screen coordinates.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `x` | int | _(required)_ | X screen coordinate |
| `y` | int | _(required)_ | Y screen coordinate |
| `button` | string | `"left"` | `"left"`, `"right"`, or `"middle"` |
| `click_type` | string | `"single"` | `"single"`, `"double"`, or `"triple"` |
| `app_name` | string | _(server target)_ | Target application |
| `pid` | int | _(server target)_ | Target process ID |

**Response:**
```json
{
  "success": true,
  "x": 500,
  "y": 300,
  "button": "left",
  "click_type": "single"
}
```

---

### `POST /type`

Type text into the focused application.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `text` | string | _(required)_ | Text to type |
| `app_name` | string | _(server target)_ | Target application |
| `pid` | int | _(server target)_ | Target process ID |
| `use_clipboard` | bool | `false` | Use clipboard paste (Ctrl+V) instead of synthetic keystrokes |

**Response:**
```json
{
  "success": true,
  "characters_typed": 22,
  "method": "sendInput"
}
```

`method` is `"sendInput"` for synthetic keystrokes or `"clipboard"` when `use_clipboard` is true.

---

### `POST /key`

Press a key with optional modifier keys.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `key` | string | _(required)_ | Key name (see [Key Names](#key-names)) |
| `modifiers` | string[] | `[]` | Modifier names (see [Modifiers](#modifiers)) |
| `app_name` | string | _(server target)_ | Target application |
| `pid` | int | _(server target)_ | Target process ID |

**Response:**
```json
{
  "success": true,
  "key": "s",
  "modifiers": ["ctrl"]
}
```

---

### `POST /scroll`

Scroll at screen coordinates.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `x` | int | _(required)_ | X screen coordinate |
| `y` | int | _(required)_ | Y screen coordinate |
| `delta_x` | int | `0` | Horizontal scroll (positive = right) |
| `delta_y` | int | `0` | Vertical scroll (positive = up, negative = down) |
| `app_name` | string | _(server target)_ | Target application |
| `pid` | int | _(server target)_ | Target process ID |

**Response:**
```json
{
  "success": true,
  "x": 500,
  "y": 300,
  "delta_x": 0,
  "delta_y": -3
}
```

---

### `POST /wait`

Pause execution for a specified duration.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `seconds` | float | `1.0` | Duration to wait |

**Response:**
```json
{
  "success": true,
  "waited_seconds": 1.0
}
```

---

### `GET /accessibility/tree`

Get the UI Automation element tree for a window.

**Query parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `app_name` | string | _(server target)_ | Application name |
| `pid` | int | _(server target)_ | Process ID |
| `format` | string | `"full"` | `"full"` or `"compact"` |
| `max_depth` | int | `10` | Maximum tree traversal depth |

`"compact"` format returns only `role`, `title`, `value`, `path`, and `children`. `"full"` adds `description`, `position`, `size`, `actions`, `is_enabled`, and `is_focused`.

**Response:**
```json
{
  "app_name": "Notepad",
  "pid": 12345,
  "tree": {
    "role": "AXWindow",
    "title": "Untitled - Notepad",
    "children": [
      {
        "role": "AXTextArea",
        "title": "Text editor",
        "value": "Hello",
        "path": [0, 0],
        "position": { "x": 100, "y": 200 },
        "size": { "width": 800, "height": 600 },
        "actions": ["setValue", "focus"],
        "is_enabled": true,
        "is_focused": true
      }
    ],
    "path": []
  }
}
```

---

### `GET /accessibility/search`

Search for UI elements matching criteria.

**Query parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `app_name` | string | _(server target)_ | Application name |
| `pid` | int | _(server target)_ | Process ID |
| `role` | string | | Filter by AX role (e.g. `AXButton`) |
| `title` | string | | Filter by exact title |
| `title_contains` | string | | Filter by title substring |
| `value` | string | | Filter by exact value |
| `max_results` | int | `50` | Maximum results to return |
| `max_depth` | int | `10` | Maximum search depth |

All filters are case-insensitive. Multiple filters are ANDed together.

**Response:**
```json
{
  "results": [
    {
      "role": "AXButton",
      "title": "Save",
      "path": [2, 0, 4],
      "position": { "x": 500, "y": 50 },
      "size": { "width": 80, "height": 30 },
      "actions": ["press", "focus"],
      "is_enabled": true,
      "is_focused": false
    }
  ],
  "count": 1
}
```

---

### `POST /accessibility/action`

Perform an action on a UI element identified by its path.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `app_name` | string | _(server target)_ | Application name |
| `pid` | int | _(server target)_ | Process ID |
| `path` | int[] | `[]` | Child index path to the element |
| `action` | string | `"press"` | Action to perform (see [Actions](#accessibility-actions)) |
| `value` | string | | Value for `setValue` action |

The `path` is an array of child indices navigating from the window root. Use `/accessibility/tree` or `/accessibility/search` to discover paths.

**Response:**
```json
{
  "success": true,
  "action": "press",
  "element_role": "AXButton",
  "element_title": "Save"
}
```

---

### `GET /menu/list`

List all menu items for an application.

**Query parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `app_name` | string | _(server target)_ | Application name |
| `pid` | int | _(server target)_ | Process ID |

**Response:**
```json
{
  "app_name": "Notepad",
  "pid": 12345,
  "menus": [
    {
      "title": "File",
      "enabled": true,
      "children": [
        { "title": "New Tab", "enabled": true, "shortcut": "Ctrl+N" },
        { "title": "New Window", "enabled": true, "shortcut": "Ctrl+Shift+N" },
        { "title": "Open", "enabled": true, "shortcut": "Ctrl+O" }
      ]
    },
    {
      "title": "Edit",
      "enabled": true,
      "children": [
        { "title": "Undo", "enabled": true, "shortcut": "Ctrl+Z" },
        { "title": "Cut", "enabled": false, "shortcut": "Ctrl+X" }
      ]
    }
  ]
}
```

---

### `POST /menu/trigger`

Trigger a menu item by navigating a path of menu titles.

**Request body:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `path` | string[] | _(required)_ | Menu title path, e.g. `["File", "New Tab"]` |
| `app_name` | string | _(server target)_ | Application name |
| `pid` | int | _(server target)_ | Process ID |

Menu titles are matched case-insensitively. Partial matches are used as fallback.

**Response:**
```json
{
  "success": true,
  "menu_path": ["File", "New Tab"]
}
```

---

### Error Responses

All errors return JSON with an `error` field:

```json
{
  "error": "internal_error",
  "detail": "No visible window found for app 'nonexistent'"
}
```

| Status | Error | When |
|--------|-------|------|
| 400 | `invalid_request` | Missing or malformed request body |
| 500 | `internal_error` | Any unhandled exception |

---

## Key Names

All key names are case-insensitive and match the macOS API contract.

### Letters and Numbers
`a`-`z`, `0`-`9`

### Function Keys
`f1`-`f20`

### Special Keys
| Key Name | Aliases | Windows Key |
|----------|---------|-------------|
| `return` | `enter` | Enter |
| `tab` | | Tab |
| `space` | | Space |
| `delete` | | Backspace |
| `forwarddelete` | | Delete |
| `escape` | `esc` | Escape |
| `capslock` | | Caps Lock |

### Navigation
| Key Name | Aliases | Windows Key |
|----------|---------|-------------|
| `uparrow` | `up` | Up Arrow |
| `downarrow` | `down` | Down Arrow |
| `leftarrow` | `left` | Left Arrow |
| `rightarrow` | `right` | Right Arrow |
| `home` | | Home |
| `end` | | End |
| `pageup` | | Page Up |
| `pagedown` | | Page Down |

### Symbols
| Key Name | Character | Key Name | Character |
|----------|-----------|----------|-----------|
| `minus` | `-` | `equal` | `=` |
| `leftbracket` | `[` | `rightbracket` | `]` |
| `backslash` | `\` | `semicolon` | `;` |
| `quote` | `'` | `comma` | `,` |
| `period` | `.` | `slash` | `/` |
| `grave` | `` ` `` | | |

### Numpad
`numpad0`-`numpad9`

### Other
`insert`, `printscreen`, `scrolllock`, `numlock`, `pause`

---

## Modifiers

| Modifier Name | Aliases | Windows Key |
|---------------|---------|-------------|
| `cmd` | `command`, `win` | Windows key |
| `ctrl` | `control` | Ctrl |
| `alt` | `option` | Alt |
| `shift` | | Shift |
| `fn` | | _(ignored — no Windows equivalent)_ |

Note: `cmd`/`command` maps to the Windows key, matching the macOS convention where Command is the primary modifier.

---

## Accessibility Actions

| Action | Description | Requires |
|--------|-------------|----------|
| `press` / `click` | Invoke, toggle, or select the element | InvokePattern, TogglePattern, or SelectionItemPattern |
| `setValue` / `set_value` | Set the element's text value | ValuePattern + `value` field |
| `focus` | Set keyboard focus to the element | _(always available)_ |
| `expand` | Expand a collapsed element (menu, combo box, tree node) | ExpandCollapsePattern |
| `collapse` | Collapse an expanded element | ExpandCollapsePattern |
| `scroll` | Scroll within the element | ScrollPattern |

---

## AX Role Mapping

Windows UI Automation ControlTypes are mapped to macOS AX-style role strings for wire compatibility:

| AX Role | Windows ControlType(s) |
|---------|----------------------|
| `AXButton` | Button |
| `AXCheckBox` | CheckBox |
| `AXComboBox` | ComboBox |
| `AXGroup` | Group, Pane, Calendar, Custom, Header |
| `AXImage` | Image |
| `AXLink` | Hyperlink |
| `AXList` | List |
| `AXMenu` | Menu |
| `AXMenuBar` | MenuBar |
| `AXMenuItem` | MenuItem |
| `AXOutline` | Tree |
| `AXPopUpButton` | SplitButton |
| `AXProgressIndicator` | ProgressBar |
| `AXRadioButton` | RadioButton, TabItem |
| `AXRow` | DataItem, TreeItem |
| `AXScrollBar` | ScrollBar |
| `AXSlider` | Slider |
| `AXSplitter` | Separator |
| `AXStaticText` | Text, ListItem, HeaderItem, StatusBar, TitleBar |
| `AXTabGroup` | Tab |
| `AXTable` | DataGrid, Table |
| `AXTextArea` | Document |
| `AXTextField` | Edit |
| `AXToolbar` | ToolBar |
| `AXWindow` | Window |
| `AXHandle` | Thumb |
| `AXHelpTag` | ToolTip |
| `AXIncrementor` | Spinner |

Unmapped control types fall back to `AXGroup`.

---

## System Tray App

Run `GeisterhandTray.exe` for a persistent system tray icon.

- Auto-starts the HTTP server on port 7676
- Status indicator: green circle = running, gray = stopped
- Right-click context menu: server status, Start/Stop/Restart, Quit
- Double-click to toggle start/stop
- Polls server status every 2 seconds

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Geisterhand.Cli / Geisterhand.Tray                        │
│  (entry points)                                             │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│  Geisterhand.Core                                           │
│                                                             │
│  ┌─────────────────┐  ┌──────────────────┐                  │
│  │ GeisterhandServer│  │  ServerManager   │                  │
│  │ (ASP.NET Core)  │  │  (lifecycle)     │                  │
│  └────────┬────────┘  └──────────────────┘                  │
│           │                                                 │
│  ┌────────▼──────────────────────────────────────┐          │
│  │ Routes: Status, Screenshot, Click, Type,      │          │
│  │         Key, Scroll, Wait, Accessibility, Menu│          │
│  └────────┬──────────────────────────────────────┘          │
│           │                                                 │
│  ┌────────▼────────┐ ┌────────────────┐ ┌────────────────┐  │
│  │ KeyboardController│ MouseController│ │ScreenCapture   │  │
│  │ (SendInput/PM)  │ │(SendInput/PM) │ │(BitBlt/Print)  │  │
│  └─────────────────┘ └────────────────┘ └────────────────┘  │
│                                                             │
│  ┌─────────────────┐ ┌────────────────┐ ┌────────────────┐  │
│  │AccessibilityService│ MenuService   │ │PermissionMgr   │  │
│  │ (UI Automation) │ │(UIA menus)    │ │(admin check)   │  │
│  └─────────────────┘ └────────────────┘ └────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Native P/Invoke: User32.cs, Kernel32.cs, Gdi32.cs      ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

**Input:** Keyboard and mouse input use Win32 `SendInput` for global input (works on whichever window is focused) and `PostMessage` for window-targeted input.

**Screen Capture:** Full-screen capture uses `BitBlt` from the desktop DC. Window capture uses `PrintWindow` with `PW_RENDERFULLCONTENT`, falling back to `BitBlt` from the window DC.

**Accessibility:** Uses System.Windows.Automation (managed UIA wrapper). Traverses the element tree, maps ControlTypes to AX-style role strings, and performs actions via UIA patterns (InvokePattern, ValuePattern, ExpandCollapsePattern, etc.).

**Window Discovery:** `EnumWindows` + `GetWindowThreadProcessId` to find windows by PID. Process name and window title matching for app name lookups. `Process.Start` with `UseShellExecute` for launching apps.

---

## Notes

- **Permissions:** Windows does not require macOS-style permission dialogs. Accessibility and screen capture work by default. Administrator privileges may be needed to interact with elevated (admin) processes.
- **Coordinates:** Both macOS accessibility coordinates and Windows screen coordinates use top-left origin. No coordinate translation is needed.
- **Bundle identifiers:** Windows has no bundle ID concept. The `bundle_identifier` field returns the full executable path, or null if the path cannot be determined.
- **Thread safety:** Services use `lock` and `SemaphoreSlim` where needed. The ASP.NET Core server handles concurrent requests.

---

## Building from Source

```bash
dotnet build       # Build all 4 projects
dotnet test        # Run 58 unit tests
```

Tests cover JSON serialization round-trips (all model types), key code mapping (letters, numbers, function keys, navigation, modifiers, extended keys, shift-characters), and modifier resolution.
