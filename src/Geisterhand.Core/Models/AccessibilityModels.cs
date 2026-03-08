namespace Geisterhand.Core.Models;

/// <summary>
/// Internal models used by accessibility services (not directly serialized to API).
/// </summary>

public record WindowInfo(
    IntPtr Handle,
    string Title,
    int ProcessId,
    string ProcessName,
    string? ExecutablePath,
    bool IsMaximized = false,
    bool IsMinimized = false,
    int X = 0,
    int Y = 0,
    int Width = 0,
    int Height = 0
);

public record ElementInfo(
    string Role,
    string? Title,
    string? Value,
    string? Description,
    double X,
    double Y,
    double Width,
    double Height,
    bool IsEnabled,
    bool IsFocused,
    List<string> AvailableActions,
    List<int> Path
);
