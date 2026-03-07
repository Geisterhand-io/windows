using Geisterhand.Core.Native;

namespace Geisterhand.Core.Input;

/// <summary>
/// Clicks inside a window using client-area coordinates.
/// DPI-aware: caller should call SetProcessDPIAware() first.
/// </summary>
public class WindowClicker
{
    public record ClickTarget(IntPtr Hwnd, string ClassName, string Text, int CtrlId);

    /// <summary>
    /// Make this process DPI-aware so window coordinates match physical pixels.
    /// </summary>
    public static void EnsureDpiAware()
    {
        User32.SetProcessDPIAware();
    }

    /// <summary>
    /// Find the deepest child HWND at the given client-area coordinates of the parent window.
    /// </summary>
    public ClickTarget FindTargetAt(IntPtr parentHwnd, int clientX, int clientY)
    {
        var screenPt = new User32.POINT { X = clientX, Y = clientY };
        User32.ClientToScreen(parentHwnd, ref screenPt);

        IntPtr target = FindDeepestChildScreen(parentHwnd, screenPt);
        if (target == IntPtr.Zero)
            target = parentHwnd;

        string className = User32.GetClassName(target);
        string text = User32.GetWindowTextString(target);
        int ctrlId = User32.GetDlgCtrlID(target);

        return new ClickTarget(target, className, text, ctrlId);
    }

    /// <summary>
    /// Click a target. For Button class, sends WM_COMMAND(BN_CLICKED) to the parent.
    /// For other controls, sends WM_LBUTTONDOWN/UP.
    /// </summary>
    public void Click(IntPtr mainHwnd, ClickTarget target, int clientX, int clientY)
    {
        if (target.ClassName.Equals("Button", StringComparison.OrdinalIgnoreCase) && target.CtrlId > 0)
        {
            // Send WM_COMMAND to the top-level window, not the direct parent,
            // since Perry's container windows don't forward WM_COMMAND.
            nint wParam = (nint)((User32.BN_CLICKED << 16) | (target.CtrlId & 0xFFFF));
            User32.SendMessageW(mainHwnd, User32.WM_COMMAND, wParam, target.Hwnd);
        }
        else
        {
            // Convert main window client coords to target window client coords
            var pt = new User32.POINT { X = clientX, Y = clientY };
            User32.ClientToScreen(mainHwnd, ref pt);
            User32.ScreenToClient(target.Hwnd, ref pt);

            nint lParam = (nint)((pt.Y << 16) | (pt.X & 0xFFFF));
            User32.SendMessageW(target.Hwnd, User32.WM_LBUTTONDOWN, 1, lParam);
            Thread.Sleep(30);
            User32.SendMessageW(target.Hwnd, User32.WM_LBUTTONUP, 0, lParam);
        }
    }

    /// <summary>
    /// Enumerate all child windows of a parent, returning their info.
    /// </summary>
    public List<ClickTarget> EnumChildren(IntPtr parentHwnd)
    {
        var result = new List<ClickTarget>();
        User32.EnumChildWindows(parentHwnd, (hWnd, lParam) =>
        {
            string className = User32.GetClassName(hWnd);
            string text = User32.GetWindowTextString(hWnd);
            int ctrlId = User32.GetDlgCtrlID(hWnd);
            result.Add(new ClickTarget(hWnd, className, text, ctrlId));
            return true;
        }, IntPtr.Zero);
        return result;
    }

    private static IntPtr FindDeepestChildScreen(IntPtr parent, User32.POINT screenPt)
    {
        var clientPt = new User32.POINT { X = screenPt.X, Y = screenPt.Y };
        User32.ScreenToClient(parent, ref clientPt);

        IntPtr child = User32.RealChildWindowFromPoint(parent, clientPt);
        if (child == IntPtr.Zero || child == parent)
            return parent;

        return FindDeepestChildScreen(child, screenPt);
    }
}
