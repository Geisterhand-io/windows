namespace Geisterhand.Core.Models;

/// <summary>
/// Internal models used by accessibility services (not directly serialized to API).
/// </summary>

public record WindowInfo(
    IntPtr Handle,
    string Title,
    int ProcessId,
    string ProcessName,
    string? ExecutablePath
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
