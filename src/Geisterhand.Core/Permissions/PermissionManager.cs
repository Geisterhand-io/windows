using System.Security.Principal;

namespace Geisterhand.Core.Permissions;

public static class PermissionManager
{
    /// <summary>
    /// On Windows, accessibility automation works by default.
    /// No special permission dialog is needed (unlike macOS).
    /// </summary>
    public static bool IsAccessibilityGranted => true;

    /// <summary>
    /// On Windows, screen recording works by default.
    /// No special permission dialog is needed (unlike macOS).
    /// </summary>
    public static bool IsScreenRecordingGranted => true;

    /// <summary>
    /// Check if the current process is running with administrator privileges.
    /// Admin may be needed to interact with elevated (admin) processes.
    /// </summary>
    public static bool IsRunAsAdministrator
    {
        get
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
