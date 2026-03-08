using System.Text.Json.Serialization;

namespace Geisterhand.Core.Models;

// --- Status ---

public record StatusResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("api_version")] string ApiVersion,
    [property: JsonPropertyName("permissions")] PermissionsInfo Permissions,
    [property: JsonPropertyName("running_applications")] List<RunningApplication> RunningApplications
);

public record PermissionsInfo(
    [property: JsonPropertyName("accessibility")] bool Accessibility,
    [property: JsonPropertyName("screen_recording")] bool ScreenRecording
);

public record RunningApplication(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("bundle_identifier")] string? BundleIdentifier,
    [property: JsonPropertyName("pid")] int Pid,
    [property: JsonPropertyName("is_active")] bool IsActive
);

// --- Screenshot ---

public record ScreenshotRequest(
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("format")] string Format = "png",
    [property: JsonPropertyName("quality")] int Quality = 85
);

public record ScreenshotResponse(
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height
);

// --- Click ---

public record ClickRequest(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("button")] string Button = "left",
    [property: JsonPropertyName("click_type")] string ClickType = "single",
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record ClickResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("button")] string Button,
    [property: JsonPropertyName("click_type")] string ClickType
);

// --- Type ---

public record TypeRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("use_clipboard")] bool UseClipboard = false,
    [property: JsonPropertyName("bundle_identifier")] string? BundleIdentifier = null
);

public record TypeResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("characters_typed")] int CharactersTyped,
    [property: JsonPropertyName("method")] string Method
);

// --- Key ---

public record KeyRequest(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("modifiers")] List<string>? Modifiers = null,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record KeyResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("modifiers")] List<string> Modifiers
);

// --- Scroll ---

public record ScrollRequest(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("delta_x")] int DeltaX = 0,
    [property: JsonPropertyName("delta_y")] int DeltaY = 0,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record ScrollResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("delta_x")] int DeltaX,
    [property: JsonPropertyName("delta_y")] int DeltaY
);

// --- Wait ---

public record WaitRequest(
    [property: JsonPropertyName("seconds")] double Seconds = 1.0
);

public record WaitResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("waited_seconds")] double WaitedSeconds
);

// --- Accessibility ---

public record AccessibilityTreeRequest(
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("format")] string Format = "full",
    [property: JsonPropertyName("max_depth")] int MaxDepth = 10
);

public record AccessibilityElement(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("position")] ElementPosition? Position = null,
    [property: JsonPropertyName("size")] ElementSize? Size = null,
    [property: JsonPropertyName("children")] List<AccessibilityElement>? Children = null,
    [property: JsonPropertyName("path")] List<int>? Path = null,
    [property: JsonPropertyName("actions")] List<string>? Actions = null,
    [property: JsonPropertyName("is_enabled")] bool? IsEnabled = null,
    [property: JsonPropertyName("is_focused")] bool? IsFocused = null,
    [property: JsonPropertyName("automation_id")] string? AutomationId = null,
    [property: JsonPropertyName("class_name")] string? ClassName = null,
    [property: JsonPropertyName("is_expanded")] bool? IsExpanded = null,
    [property: JsonPropertyName("is_selected")] bool? IsSelected = null,
    [property: JsonPropertyName("is_checked")] bool? IsChecked = null,
    [property: JsonPropertyName("is_offscreen")] bool? IsOffscreen = null,
    [property: JsonPropertyName("control_type")] string? ControlType = null,
    [property: JsonPropertyName("process_id")] int? ProcessId = null
);

public record ElementPosition(
    [property: JsonPropertyName("x")] double X,
    [property: JsonPropertyName("y")] double Y
);

public record ElementSize(
    [property: JsonPropertyName("width")] double Width,
    [property: JsonPropertyName("height")] double Height
);

public record AccessibilityTreeResponse(
    [property: JsonPropertyName("app_name")] string AppName,
    [property: JsonPropertyName("pid")] int Pid,
    [property: JsonPropertyName("tree")] AccessibilityElement Tree
);

public record AccessibilityActionRequest(
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("path")] List<int>? Path = null,
    [property: JsonPropertyName("action")] string Action = "press",
    [property: JsonPropertyName("value")] string? Value = null
);

public record AccessibilityActionResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("element_role")] string? ElementRole = null,
    [property: JsonPropertyName("element_title")] string? ElementTitle = null
);

public record AccessibilitySearchRequest(
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("role")] string? Role = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("title_contains")] string? TitleContains = null,
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("max_results")] int MaxResults = 50,
    [property: JsonPropertyName("max_depth")] int MaxDepth = 10,
    [property: JsonPropertyName("title_regex")] string? TitleRegex = null,
    [property: JsonPropertyName("value_regex")] string? ValueRegex = null,
    [property: JsonPropertyName("automation_id")] string? AutomationId = null,
    [property: JsonPropertyName("enabled_only")] bool? EnabledOnly = null,
    [property: JsonPropertyName("visible_only")] bool? VisibleOnly = null
);

public record AccessibilitySearchResponse(
    [property: JsonPropertyName("results")] List<AccessibilityElement> Results,
    [property: JsonPropertyName("count")] int Count
);

// --- Menu ---

public record MenuListRequest(
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record MenuItem(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("enabled")] bool Enabled = true,
    [property: JsonPropertyName("shortcut")] string? Shortcut = null,
    [property: JsonPropertyName("children")] List<MenuItem>? Children = null
);

public record MenuListResponse(
    [property: JsonPropertyName("app_name")] string AppName,
    [property: JsonPropertyName("pid")] int Pid,
    [property: JsonPropertyName("menus")] List<MenuItem> Menus
);

public record MenuTriggerRequest(
    [property: JsonPropertyName("path")] List<string> Path,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record MenuTriggerResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("menu_path")] List<string> MenuPath
);

// --- Run ---

public record RunResponse(
    [property: JsonPropertyName("app_name")] string AppName,
    [property: JsonPropertyName("pid")] int Pid,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("base_url")] string BaseUrl
);

// --- Mouse Move ---

public record MouseMoveRequest(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record MouseMoveResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y
);

// --- Hover ---

public record HoverRequest(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("hover_duration_ms")] int HoverDurationMs = 500,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record HoverResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("hover_duration_ms")] int HoverDurationMs
);

// --- Drag ---

public record DragRequest(
    [property: JsonPropertyName("start_x")] int StartX,
    [property: JsonPropertyName("start_y")] int StartY,
    [property: JsonPropertyName("end_x")] int EndX,
    [property: JsonPropertyName("end_y")] int EndY,
    [property: JsonPropertyName("duration_ms")] int DurationMs = 500,
    [property: JsonPropertyName("button")] string Button = "left",
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record DragResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("start_x")] int StartX,
    [property: JsonPropertyName("start_y")] int StartY,
    [property: JsonPropertyName("end_x")] int EndX,
    [property: JsonPropertyName("end_y")] int EndY
);

// --- Window Management ---

public record WindowManageRequest(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("x")] int? X = null,
    [property: JsonPropertyName("y")] int? Y = null,
    [property: JsonPropertyName("width")] int? Width = null,
    [property: JsonPropertyName("height")] int? Height = null
);

public record WindowManageResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("x")] int? X = null,
    [property: JsonPropertyName("y")] int? Y = null,
    [property: JsonPropertyName("width")] int? Width = null,
    [property: JsonPropertyName("height")] int? Height = null,
    [property: JsonPropertyName("state")] string? State = null
);

// --- Wait For Element ---

public record WaitForElementRequest(
    [property: JsonPropertyName("role")] string? Role = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("title_contains")] string? TitleContains = null,
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("timeout_ms")] int TimeoutMs = 10000,
    [property: JsonPropertyName("poll_interval_ms")] int PollIntervalMs = 250,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null,
    [property: JsonPropertyName("max_depth")] int MaxDepth = 10
);

// --- Wait For Condition ---

public record WaitForConditionRequest(
    [property: JsonPropertyName("path")] List<int> Path,
    [property: JsonPropertyName("condition")] string Condition,
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("timeout_ms")] int TimeoutMs = 10000,
    [property: JsonPropertyName("poll_interval_ms")] int PollIntervalMs = 250,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record WaitForConditionResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("condition")] string Condition,
    [property: JsonPropertyName("elapsed_ms")] long ElapsedMs
);

// --- Clipboard ---

public record ClipboardWriteRequest(
    [property: JsonPropertyName("text")] string Text
);

public record ClipboardResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("text")] string? Text = null
);

// --- Key State ---

public record KeyStateResponse(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("pressed")] bool Pressed,
    [property: JsonPropertyName("toggled")] bool Toggled
);

// --- Text Selection ---

public record SelectTextRequest(
    [property: JsonPropertyName("start_x")] int StartX,
    [property: JsonPropertyName("start_y")] int StartY,
    [property: JsonPropertyName("end_x")] int EndX,
    [property: JsonPropertyName("end_y")] int EndY,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

// --- Highlight ---

public record HighlightRequest(
    [property: JsonPropertyName("path")] List<int>? Path = null,
    [property: JsonPropertyName("duration_ms")] int DurationMs = 2000,
    [property: JsonPropertyName("color")] string Color = "red",
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record HighlightResponse(
    [property: JsonPropertyName("success")] bool Success
);

// --- Element At Point ---

public record ElementAtPointResponse(
    [property: JsonPropertyName("element")] AccessibilityElement? Element,
    [property: JsonPropertyName("found")] bool Found
);

// --- Screenshot Diff ---

public record ScreenshotDiffRequest(
    [property: JsonPropertyName("baseline")] string Baseline,
    [property: JsonPropertyName("current")] string Current,
    [property: JsonPropertyName("threshold")] double Threshold = 0.01
);

public record ScreenshotDiffResponse(
    [property: JsonPropertyName("match")] bool Match,
    [property: JsonPropertyName("diff_percent")] double DiffPercent,
    [property: JsonPropertyName("diff_image")] string? DiffImage = null
);

// --- Navigate ---

public record NavigateRequest(
    [property: JsonPropertyName("path")] List<int> Path,
    [property: JsonPropertyName("direction")] string Direction,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

// --- Monitor Info ---

public record MonitorInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("dpi")] int Dpi,
    [property: JsonPropertyName("is_primary")] bool IsPrimary
);

public record MonitorsResponse(
    [property: JsonPropertyName("monitors")] List<MonitorInfo> Monitors
);

// --- Context Menu ---

public record ContextMenuRequest(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("app_name")] string? AppName = null,
    [property: JsonPropertyName("pid")] int? Pid = null
);

public record ContextMenuResponse(
    [property: JsonPropertyName("items")] List<MenuItem> Items
);

// --- Error ---

public record ErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("detail")] string? Detail = null
);
