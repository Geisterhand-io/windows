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
    [property: JsonPropertyName("is_focused")] bool? IsFocused = null
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
    [property: JsonPropertyName("max_depth")] int MaxDepth = 10
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

// --- Error ---

public record ErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("detail")] string? Detail = null
);
