# Geisterhand Windows — Feature Plan

This plan covers all missing features, organized into implementation phases.
Each item includes the files to create/modify and a brief implementation note.

---

## Phase 1: Core Testing Primitives (High Value)

These are needed for any real UI test scenario (e.g., testing Hone IDE).

### 1.1 `wait_for_element` — Block until UIA element appears

**CLI:** `geisterhand wait-for --role Button --title "Save" --timeout 10`
**API:** `POST /accessibility/wait` `{role, title, title_contains, value, timeout_ms, poll_interval_ms}`

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `WaitForElement(root, criteria, timeoutMs, pollIntervalMs)` method that polls `Search()` in a loop with `Task.Delay`
- `src/Geisterhand.Core/Server/Routes/AccessibilityRoute.cs` — Add `POST /accessibility/wait` endpoint
- `src/Geisterhand.Cli/Commands/WaitForCommand.cs` — New CLI command
- `src/Geisterhand.Cli/Program.cs` — Register command
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `WaitForRequest`/`WaitForResponse` records

**Behavior:**
- Poll every `poll_interval_ms` (default 250ms) up to `timeout_ms` (default 10000ms)
- Return the first matching element (same format as `/accessibility/search`)
- Return 408 Timeout if not found within deadline
- Support same filters as search: role, title, title_contains, value

---

### 1.2 `wait_for_condition` — Poll until element matches state

**API:** `POST /accessibility/wait-condition` `{path, condition, value, timeout_ms}`

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `WaitForCondition(root, path, condition, value, timeoutMs)` method
- `src/Geisterhand.Core/Server/Routes/AccessibilityRoute.cs` — Add endpoint

**Conditions:**
- `enabled` — Element.IsEnabled == true
- `disabled` — Element.IsEnabled == false
- `expanded` — ExpandCollapsePattern.Current.ExpandCollapseState == Expanded
- `collapsed` — ExpandCollapsePattern.Current.ExpandCollapseState == Collapsed
- `value_equals` — ValuePattern.Current.Value == expected
- `value_contains` — ValuePattern.Current.Value.Contains(expected)
- `exists` — Element is in tree (same as wait_for_element but by path)
- `not_exists` — Element no longer in tree (for waiting on dialogs to close)

---

### 1.3 Mouse move / hover

**CLI:** `geisterhand mouse-move <x> <y> [--app-name X]`
**API:** `POST /mouse-move` `{x, y, app_name, pid}`

**Files:**
- `src/Geisterhand.Core/Input/MouseController.cs` — Add `MoveTo(x, y)` and `MoveToWindow(hwnd, x, y)` using `SetCursorPos` / `SendInput(MOUSEEVENTF_MOVE)`
- `src/Geisterhand.Core/Server/Routes/MouseMoveRoute.cs` — New route
- `src/Geisterhand.Cli/Commands/MouseMoveCommand.cs` — New CLI command
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `MouseMoveRequest`

**Also add:** `POST /hover` alias that moves mouse and waits 500ms (configurable `hover_duration_ms`) for tooltip to appear.

---

### 1.4 Drag and drop

**CLI:** `geisterhand drag <x1> <y1> <x2> <y2> [--duration 500] [--app-name X]`
**API:** `POST /drag` `{start_x, start_y, end_x, end_y, duration_ms, button, app_name, pid}`

**Files:**
- `src/Geisterhand.Core/Input/MouseController.cs` — Add `Drag(startX, startY, endX, endY, durationMs, button)` using SendInput: MOUSEEVENTF_LEFTDOWN at start, interpolate MOUSEEVENTF_MOVE over duration, MOUSEEVENTF_LEFTUP at end
- `src/Geisterhand.Core/Server/Routes/DragRoute.cs` — New route
- `src/Geisterhand.Cli/Commands/DragCommand.cs` — New CLI command
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `DragRequest`

**Implementation:**
- Linear interpolation from (x1,y1) to (x2,y2) over N steps
- Default 500ms duration, ~20 intermediate move events
- Support left/right/middle button

---

### 1.5 Window management (resize, move, minimize, maximize, close)

**CLI:** `geisterhand window <action> [--app-name X] [--pid N] [--x 100] [--y 100] [--width 800] [--height 600]`
**API:** `POST /window` `{action, app_name, pid, x, y, width, height}`

**Files:**
- `src/Geisterhand.Core/Screen/WindowManager.cs` — New service class
  - `Resize(hwnd, width, height)` — `MoveWindow` or `SetWindowPos`
  - `Move(hwnd, x, y)` — `SetWindowPos` with SWP_NOSIZE
  - `Maximize(hwnd)` — `ShowWindow(SW_MAXIMIZE)`
  - `Minimize(hwnd)` — `ShowWindow(SW_MINIMIZE)`
  - `Restore(hwnd)` — `ShowWindow(SW_RESTORE)`
  - `Close(hwnd)` — `SendMessage(WM_CLOSE)`
  - `GetRect(hwnd)` — `GetWindowRect` → {x, y, width, height}
  - `GetState(hwnd)` — Query IsIconic/IsZoomed → "normal"/"maximized"/"minimized"
- `src/Geisterhand.Core/Native/User32.cs` — Add `SetWindowPos`, `IsIconic`, `IsZoomed` P/Invoke declarations
- `src/Geisterhand.Core/Server/Routes/WindowRoute.cs` — New route
- `src/Geisterhand.Cli/Commands/WindowCommand.cs` — New CLI command
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `WindowRequest`/`WindowResponse`

**Actions:** `resize`, `move`, `maximize`, `minimize`, `restore`, `close`, `get-rect`, `get-state`

---

### 1.6 Element state in accessibility tree

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Extend `BuildElementInfo()` to include:
  - `automation_id` — `AutomationElement.Current.AutomationId`
  - `class_name` — `AutomationElement.Current.ClassName`
  - `is_expanded` — via ExpandCollapsePattern
  - `is_selected` — via SelectionItemPattern
  - `is_checked` — via TogglePattern (`ToggleState`)
  - `is_offscreen` — `AutomationElement.Current.IsOffscreen`
  - `control_type` — raw UIA control type name
  - `process_id` — `AutomationElement.Current.ProcessId`
- `src/Geisterhand.Core/Models/ApiModels.cs` — Extend `ElementInfo` record with new nullable fields

**Non-breaking:** New fields are nullable, existing clients see no difference.

---

### 1.7 Clipboard read/write

**CLI:** `geisterhand clipboard read` / `geisterhand clipboard write <text>`
**API:** `GET /clipboard` / `POST /clipboard` `{text}`

**Files:**
- `src/Geisterhand.Core/Input/ClipboardService.cs` — New service
  - `GetText()` — `Clipboard.GetText()` (requires STA thread)
  - `SetText(text)` — `Clipboard.SetText(text)`
  - Wrap in STA thread dispatch if needed (WPF Dispatcher or manual Thread with ApartmentState)
- `src/Geisterhand.Core/Server/Routes/ClipboardRoute.cs` — New route
- `src/Geisterhand.Cli/Commands/ClipboardCommand.cs` — New CLI command
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `ClipboardResponse` record

---

## Phase 2: Advanced Element Finding

### 2.1 Regex search

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `title_regex` and `value_regex` parameters to `Search()`. Use `System.Text.RegularExpressions.Regex.IsMatch()`.
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add `title_regex`/`value_regex` to `AccessibilitySearchRequest`

---

### 2.2 Find by automationId

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `automation_id` parameter to `Search()`. Match against `AutomationElement.Current.AutomationId`.
- `src/Geisterhand.Core/Models/ApiModels.cs` — Add field to search request

---

### 2.3 Filter by state (visible/enabled only)

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `enabled_only` and `visible_only` (not offscreen) boolean parameters to `Search()`.

---

### 2.4 Ancestor / sibling traversal

**API:** `GET /accessibility/navigate` `{path, direction}` where direction = `parent`, `next_sibling`, `prev_sibling`, `first_child`, `last_child`

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `Navigate(root, path, direction)` using `TreeWalker.RawViewWalker` methods
- `src/Geisterhand.Core/Server/Routes/AccessibilityRoute.cs` — Add endpoint

---

## Phase 3: Visual Debugging

### 3.1 Element highlight (draw red border)

**CLI:** `geisterhand highlight --path [0,1,3] --duration 2 [--app-name X]`
**API:** `POST /accessibility/highlight` `{path, duration_ms, color}`

**Files:**
- `src/Geisterhand.Core/Accessibility/HighlightOverlay.cs` — New class
  - Creates a transparent top-most layered window (`WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST`)
  - Draws a colored border (default red, 3px) at the element's bounding rect
  - Auto-dismiss after duration (default 2s)
  - Uses `CreateWindowEx` + `SetLayeredWindowAttributes` for click-through transparency
- `src/Geisterhand.Core/Server/Routes/AccessibilityRoute.cs` — Add endpoint
- `src/Geisterhand.Cli/Commands/HighlightCommand.cs` — New CLI command

---

### 3.2 Element pick (inspect under cursor)

**CLI:** `geisterhand inspect` (moves cursor, press Enter to select, prints element info)
**API:** `GET /accessibility/at-point?x=100&y=200` — Return element at screen coordinates

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Add `GetElementAtPoint(x, y)` using `AutomationElement.FromPoint(new Point(x, y))`
- `src/Geisterhand.Core/Server/Routes/AccessibilityRoute.cs` — Add endpoint
- `src/Geisterhand.Cli/Commands/InspectCommand.cs` — Interactive CLI (poll cursor + display element info)

---

### 3.3 Screenshot comparison / visual diff

**CLI:** `geisterhand screenshot-diff <baseline.png> <current.png> --threshold 0.01 --output diff.png`
**API:** `POST /screenshot/diff` (multipart: two images + threshold)

**Files:**
- `src/Geisterhand.Core/Screen/ImageDiffService.cs` — New service
  - `Compare(bitmap1, bitmap2, threshold)` → `{match: bool, diff_percent: float, diff_image: Bitmap}`
  - Pixel-by-pixel comparison, highlight differences in red
  - Threshold: percentage of pixels that can differ (default 0.01 = 1%)
- `src/Geisterhand.Core/Server/Routes/ScreenshotRoute.cs` — Add diff endpoint
- `src/Geisterhand.Cli/Commands/ScreenshotDiffCommand.cs` — New CLI command

---

## Phase 4: Context Menus & Advanced Menus

### 4.1 Right-click context menu inspection

**API:** `POST /menu/context` `{x, y, app_name}` — Right-click at coordinates, wait for context menu, return structured menu tree

**Files:**
- `src/Geisterhand.Core/Accessibility/MenuService.cs` — Add `GetContextMenu(hwnd, x, y)`
  - Send WM_RBUTTONDOWN/UP at coordinates
  - Wait ~200ms for menu popup
  - Find popup menu via UIA (`ControlType.Menu` that appeared after click)
  - Build tree same as main menu
  - Return items
- `src/Geisterhand.Core/Server/Routes/MenuRoute.cs` — Add endpoint

---

## Phase 5: Resilience & Robustness

### 5.1 Implicit retries on transient failures

**Files:**
- `src/Geisterhand.Core/Accessibility/RetryPolicy.cs` — New helper
  - Wrap UIA calls in retry with exponential backoff
  - Catch `ElementNotAvailableException`, `InvalidOperationException`
  - Default: 3 retries, 100ms/200ms/400ms delays
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — Wrap tree/search/action calls with retry policy

---

### 5.2 Stale element recovery

**Files:**
- `src/Geisterhand.Core/Accessibility/AccessibilityService.cs` — When navigating to a path fails (element removed during UI update), retry from root with the same path after a short delay.

---

### 5.3 Request logging

**Files:**
- `src/Geisterhand.Core/Server/GeisterhandServer.cs` — Add ASP.NET Core middleware that logs:
  - Request method + path + body summary
  - Response status code + duration
  - Use `ILogger` with structured log format
  - Log to file (`geisterhand.log`) and stdout
  - Add `--verbose` flag to server/run commands

---

## Phase 6: Advanced Input

### 6.1 Keyboard layout awareness

**Files:**
- `src/Geisterhand.Core/Input/KeyboardController.cs` — Use `VkKeyScanEx` to resolve characters to the active keyboard layout's VK+shift state, instead of assuming US QWERTY.
- `src/Geisterhand.Core/Native/User32.cs` — Add `VkKeyScanEx`, `GetKeyboardLayout` P/Invoke

---

### 6.2 Keyboard state query

**CLI:** `geisterhand key-state [key]`
**API:** `GET /key-state?key=capslock`

**Files:**
- `src/Geisterhand.Core/Input/KeyboardController.cs` — Add `GetKeyState(keyName)` using `GetKeyState` / `GetAsyncKeyState` P/Invoke
- `src/Geisterhand.Core/Native/User32.cs` — Add P/Invoke declarations
- `src/Geisterhand.Core/Server/Routes/KeyRoute.cs` — Add GET endpoint
- `src/Geisterhand.Cli/Commands/KeyStateCommand.cs` — New CLI command

Returns: `{key, pressed: bool, toggled: bool}` (toggled = caps lock on, etc.)

---

### 6.3 Text selection

**API:** `POST /select-text` `{start_x, start_y, end_x, end_y}` — Click + shift-click to select range

**Files:**
- `src/Geisterhand.Core/Input/MouseController.cs` — Add `SelectRange(startX, startY, endX, endY)`:
  1. Click at (startX, startY)
  2. SendInput: Shift down
  3. Click at (endX, endY)
  4. SendInput: Shift up

Alternative: triple-click for line select, Ctrl+A for all.

---

## Phase 7: Multi-Monitor & Advanced Window

### 7.1 Multi-monitor support

**API:** `GET /monitors` — List all monitors with bounds and DPI

**Files:**
- `src/Geisterhand.Core/Screen/MonitorService.cs` — New service
  - `GetMonitors()` using `EnumDisplayMonitors` P/Invoke
  - Return: `[{name, x, y, width, height, dpi, is_primary}]`
- `src/Geisterhand.Core/Native/User32.cs` — Add `EnumDisplayMonitors`, `GetMonitorInfo`, `GetDpiForMonitor`
- `src/Geisterhand.Core/Server/Routes/StatusRoute.cs` — Extend `/status` response with monitor info
- `src/Geisterhand.Core/Screen/ScreenCaptureService.cs` — Add `CaptureMonitor(monitorIndex)` for per-monitor screenshots

---

### 7.2 Window state queries in status

**Files:**
- `src/Geisterhand.Core/Screen/ScreenCaptureService.cs` — Extend `GetVisibleWindows()` to include:
  - `is_maximized`, `is_minimized`
  - `x`, `y`, `width`, `height` (window rect)
  - `monitor_index`
- `src/Geisterhand.Core/Models/ApiModels.cs` — Extend `WindowInfo`

---

## Phase 8: Event Listening & OCR

### 8.1 UIA event subscriptions

**API:** `POST /accessibility/subscribe` `{event_type, path, callback_url}` or WebSocket stream

**Files:**
- `src/Geisterhand.Core/Accessibility/EventListener.cs` — New class
  - `Subscribe(element, eventType)` using `Automation.AddAutomationEventHandler` / `AddStructureChangedEventHandler` / `AddAutomationPropertyChangedEventHandler`
  - Event types: `structure_changed`, `property_changed`, `focus_changed`
  - Deliver via WebSocket (preferred) or callback POST
- `src/Geisterhand.Core/Server/GeisterhandServer.cs` — Add WebSocket endpoint `/events`

**Complexity:** High. UIA events require COM apartment thread management. Consider Phase 8+ only if polling proves insufficient.

---

### 8.2 OCR (text from screenshot regions)

**CLI:** `geisterhand ocr --region 100,200,400,300 [--app-name X]`
**API:** `POST /ocr` `{x, y, width, height, app_name}`

**Files:**
- `src/Geisterhand.Core/Screen/OcrService.cs` — New service
  - Use Windows.Media.Ocr (WinRT) or Tesseract.NET
  - Capture region screenshot, run OCR, return text + bounding boxes
  - WinRT OCR is built-in on Windows 10+ (no external dependency)
- `src/Geisterhand.Core/Server/Routes/OcrRoute.cs` — New route
- `src/Geisterhand.Cli/Commands/OcrCommand.cs` — New CLI command

Returns: `{text: "full text", regions: [{text, x, y, width, height}]}`

---

## Summary — Implementation Order

| Phase | Items | Effort | Impact |
|-------|-------|--------|--------|
| **1** | wait_for_element, wait_for_condition, mouse move, drag, window mgmt, element state, clipboard | 3-4 days | Unlocks real test automation |
| **2** | Regex search, automationId, state filters, ancestor traversal | 1-2 days | Better element finding |
| **3** | Highlight, inspect at point, screenshot diff | 2-3 days | Visual debugging |
| **4** | Context menu inspection | 1 day | Full menu coverage |
| **5** | Retries, stale recovery, logging | 1-2 days | Production robustness |
| **6** | Keyboard layout, key state, text selection | 1-2 days | Input completeness |
| **7** | Multi-monitor, window state | 1 day | Multi-display support |
| **8** | UIA events, OCR | 3-4 days | Advanced / optional |

**Total: ~14-20 days of implementation work.**
**Recommended start: Phase 1 (core testing primitives).**
