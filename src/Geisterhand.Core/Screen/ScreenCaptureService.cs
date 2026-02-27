using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Geisterhand.Core.Models;
using Geisterhand.Core.Native;

namespace Geisterhand.Core.Screen;

public class ScreenCaptureService
{
    /// <summary>
    /// Capture the entire primary screen.
    /// </summary>
    public Bitmap CaptureScreen()
    {
        int width = User32.GetSystemMetrics(User32.SM_CXSCREEN);
        int height = User32.GetSystemMetrics(User32.SM_CYSCREEN);

        IntPtr desktopDC = User32.GetDC(IntPtr.Zero);
        IntPtr memDC = Gdi32.CreateCompatibleDC(desktopDC);
        IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(desktopDC, width, height);
        IntPtr oldBitmap = Gdi32.SelectObject(memDC, hBitmap);

        Gdi32.BitBlt(memDC, 0, 0, width, height, desktopDC, 0, 0, Gdi32.SRCCOPY);

        Gdi32.SelectObject(memDC, oldBitmap);
        var bitmap = Image.FromHbitmap(hBitmap);

        Gdi32.DeleteObject(hBitmap);
        Gdi32.DeleteDC(memDC);
        User32.ReleaseDC(IntPtr.Zero, desktopDC);

        return bitmap;
    }

    /// <summary>
    /// Capture a specific window by handle using PrintWindow.
    /// Falls back to BitBlt if PrintWindow fails.
    /// </summary>
    public Bitmap CaptureWindow(IntPtr hWnd)
    {
        User32.GetWindowRect(hWnd, out var rect);
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Window has zero or negative dimensions.");

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        IntPtr hdc = graphics.GetHdc();

        bool success = User32.PrintWindow(hWnd, hdc, User32.PW_RENDERFULLCONTENT);
        if (!success)
        {
            // Fallback to BitBlt
            IntPtr windowDC = User32.GetDC(hWnd);
            Gdi32.BitBlt(hdc, 0, 0, width, height, windowDC, 0, 0, Gdi32.SRCCOPY);
            User32.ReleaseDC(hWnd, windowDC);
        }

        graphics.ReleaseHdc(hdc);
        return bitmap;
    }

    /// <summary>
    /// Find main window handle(s) for a given process ID.
    /// </summary>
    public IntPtr FindWindowByPid(int pid)
    {
        IntPtr foundHwnd = IntPtr.Zero;
        User32.EnumWindows((hWnd, lParam) =>
        {
            User32.GetWindowThreadProcessId(hWnd, out uint windowPid);
            if (windowPid == (uint)pid && User32.IsWindowVisible(hWnd))
            {
                string title = User32.GetWindowTextString(hWnd);
                if (!string.IsNullOrEmpty(title))
                {
                    foundHwnd = hWnd;
                    return false; // stop enumeration
                }
            }
            return true;
        }, IntPtr.Zero);
        return foundHwnd;
    }

    /// <summary>
    /// Find window handle by application name (process name or window title match).
    /// </summary>
    public IntPtr FindWindowByAppName(string appName)
    {
        // Try by process name first
        var processes = Process.GetProcessesByName(appName);
        if (processes.Length == 0)
        {
            // Try without extension
            processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appName));
        }

        foreach (var proc in processes)
        {
            if (proc.MainWindowHandle != IntPtr.Zero)
                return proc.MainWindowHandle;

            // Try EnumWindows for this PID
            var hwnd = FindWindowByPid(proc.Id);
            if (hwnd != IntPtr.Zero)
                return hwnd;
        }

        // Try by window title
        IntPtr foundHwnd = IntPtr.Zero;
        User32.EnumWindows((hWnd, lParam) =>
        {
            if (!User32.IsWindowVisible(hWnd)) return true;
            string title = User32.GetWindowTextString(hWnd);
            if (title.Contains(appName, StringComparison.OrdinalIgnoreCase))
            {
                foundHwnd = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);

        return foundHwnd;
    }

    /// <summary>
    /// Resolve a window handle from app name or PID.
    /// </summary>
    public IntPtr ResolveWindow(string? appName, int? pid)
    {
        if (pid.HasValue)
        {
            var hwnd = FindWindowByPid(pid.Value);
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException($"No visible window found for PID {pid.Value}");
            return hwnd;
        }

        if (!string.IsNullOrEmpty(appName))
        {
            var hwnd = FindWindowByAppName(appName);
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException($"No visible window found for app '{appName}'");
            return hwnd;
        }

        throw new ArgumentException("Either app_name or pid must be specified");
    }

    /// <summary>
    /// Get all visible windows as WindowInfo objects.
    /// </summary>
    public List<WindowInfo> GetVisibleWindows()
    {
        var windows = new List<WindowInfo>();
        User32.EnumWindows((hWnd, lParam) =>
        {
            if (!User32.IsWindowVisible(hWnd)) return true;
            string title = User32.GetWindowTextString(hWnd);
            if (string.IsNullOrEmpty(title)) return true;

            User32.GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                var proc = Process.GetProcessById((int)pid);
                string? exePath = null;
                try { exePath = proc.MainModule?.FileName; } catch { }

                windows.Add(new WindowInfo(
                    Handle: hWnd,
                    Title: title,
                    ProcessId: (int)pid,
                    ProcessName: proc.ProcessName,
                    ExecutablePath: exePath
                ));
            }
            catch { }
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    /// <summary>
    /// Get the process ID owning the given window handle.
    /// </summary>
    public int GetWindowProcessId(IntPtr hWnd)
    {
        User32.GetWindowThreadProcessId(hWnd, out uint pid);
        return (int)pid;
    }

    /// <summary>
    /// Ensure a window is visible and in the foreground.
    /// </summary>
    public void BringWindowToFront(IntPtr hWnd)
    {
        if (User32.IsIconic(hWnd))
            User32.ShowWindow(hWnd, User32.SW_RESTORE);
        User32.SetForegroundWindow(hWnd);
    }
}
