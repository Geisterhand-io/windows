using Geisterhand.Core.Native;

namespace Geisterhand.Core.Screen;

public class WindowManager
{
    public void Resize(IntPtr hWnd, int width, int height)
    {
        User32.GetWindowRect(hWnd, out var rect);
        User32.MoveWindow(hWnd, rect.Left, rect.Top, width, height, true);
    }

    public void Move(IntPtr hWnd, int x, int y)
    {
        User32.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, User32.SWP_NOSIZE | User32.SWP_NOZORDER);
    }

    public void MoveAndResize(IntPtr hWnd, int x, int y, int width, int height)
    {
        User32.MoveWindow(hWnd, x, y, width, height, true);
    }

    public void Maximize(IntPtr hWnd)
    {
        User32.ShowWindow(hWnd, User32.SW_MAXIMIZE);
    }

    public void Minimize(IntPtr hWnd)
    {
        User32.ShowWindow(hWnd, User32.SW_MINIMIZE);
    }

    public void Restore(IntPtr hWnd)
    {
        User32.ShowWindow(hWnd, User32.SW_RESTORE);
    }

    public void Close(IntPtr hWnd)
    {
        User32.SendMessageW(hWnd, User32.WM_CLOSE, 0, 0);
    }

    public (int x, int y, int width, int height) GetRect(IntPtr hWnd)
    {
        User32.GetWindowRect(hWnd, out var rect);
        return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public string GetState(IntPtr hWnd)
    {
        if (User32.IsIconic(hWnd)) return "minimized";
        if (User32.IsZoomed(hWnd)) return "maximized";
        return "normal";
    }
}
