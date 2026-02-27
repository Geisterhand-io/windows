using System.Runtime.InteropServices;
using Geisterhand.Core.Native;

namespace Geisterhand.Core.Input;

public class MouseController
{
    private static readonly int s_inputSize = Marshal.SizeOf<User32.INPUT>();

    /// <summary>
    /// Click at the given screen coordinates using SendInput (global).
    /// </summary>
    public void Click(int x, int y, string button = "left", string clickType = "single")
    {
        // Move cursor to position
        User32.SetCursorPos(x, y);
        Thread.Sleep(10); // small delay for cursor settle

        var (downFlag, upFlag, data) = GetButtonFlags(button);
        int count = clickType == "double" ? 2 : clickType == "triple" ? 3 : 1;

        var inputs = new List<User32.INPUT>();
        for (int i = 0; i < count; i++)
        {
            inputs.Add(MakeMouseInput(0, 0, data, downFlag));
            inputs.Add(MakeMouseInput(0, 0, data, upFlag));
        }

        var arr = inputs.ToArray();
        User32.SendInput((uint)arr.Length, arr, s_inputSize);
    }

    /// <summary>
    /// Click targeted to a specific window handle using PostMessage.
    /// Coordinates are relative to the window's client area.
    /// </summary>
    public void ClickWindow(IntPtr hWnd, int x, int y, string button = "left", string clickType = "single")
    {
        var (downMsg, upMsg) = GetPostMessagePair(button);
        nint lParam = MakePointLParam(x, y);
        int count = clickType == "double" ? 2 : clickType == "triple" ? 3 : 1;

        for (int i = 0; i < count; i++)
        {
            User32.PostMessageW(hWnd, downMsg, 0, lParam);
            User32.PostMessageW(hWnd, upMsg, 0, lParam);
        }
    }

    /// <summary>
    /// Scroll at the given screen coordinates using SendInput.
    /// </summary>
    public void Scroll(int x, int y, int deltaX, int deltaY)
    {
        User32.SetCursorPos(x, y);
        Thread.Sleep(10);

        var inputs = new List<User32.INPUT>();

        if (deltaY != 0)
        {
            inputs.Add(MakeMouseInput(0, 0, deltaY * User32.WHEEL_DELTA, User32.MOUSEEVENTF_WHEEL));
        }
        if (deltaX != 0)
        {
            inputs.Add(MakeMouseInput(0, 0, deltaX * User32.WHEEL_DELTA, User32.MOUSEEVENTF_HWHEEL));
        }

        if (inputs.Count > 0)
        {
            var arr = inputs.ToArray();
            User32.SendInput((uint)arr.Length, arr, s_inputSize);
        }
    }

    /// <summary>
    /// Scroll targeted to a specific window handle using PostMessage.
    /// </summary>
    public void ScrollWindow(IntPtr hWnd, int x, int y, int deltaX, int deltaY)
    {
        nint lParam = MakePointLParam(x, y);

        if (deltaY != 0)
        {
            nint wParam = (nint)((deltaY * User32.WHEEL_DELTA) << 16);
            User32.PostMessageW(hWnd, User32.WM_MOUSEWHEEL, wParam, lParam);
        }
        if (deltaX != 0)
        {
            nint wParam = (nint)((deltaX * User32.WHEEL_DELTA) << 16);
            User32.PostMessageW(hWnd, User32.WM_MOUSEHWHEEL, wParam, lParam);
        }
    }

    /// <summary>
    /// Get current cursor position.
    /// </summary>
    public (int X, int Y) GetCursorPosition()
    {
        User32.GetCursorPos(out var pt);
        return (pt.X, pt.Y);
    }

    private static (uint downFlag, uint upFlag, int data) GetButtonFlags(string button)
    {
        return button.ToLowerInvariant() switch
        {
            "right" => (User32.MOUSEEVENTF_RIGHTDOWN, User32.MOUSEEVENTF_RIGHTUP, 0),
            "middle" => (User32.MOUSEEVENTF_MIDDLEDOWN, User32.MOUSEEVENTF_MIDDLEUP, 0),
            _ => (User32.MOUSEEVENTF_LEFTDOWN, User32.MOUSEEVENTF_LEFTUP, 0),
        };
    }

    private static (uint downMsg, uint upMsg) GetPostMessagePair(string button)
    {
        return button.ToLowerInvariant() switch
        {
            "right" => (User32.WM_RBUTTONDOWN, User32.WM_RBUTTONUP),
            "middle" => (User32.WM_MBUTTONDOWN, User32.WM_MBUTTONUP),
            _ => (User32.WM_LBUTTONDOWN, User32.WM_LBUTTONUP),
        };
    }

    private static User32.INPUT MakeMouseInput(int dx, int dy, int mouseData, uint flags)
    {
        return new User32.INPUT
        {
            Type = User32.INPUT_MOUSE,
            U = new User32.InputUnion
            {
                mi = new User32.MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = mouseData,
                    dwFlags = flags,
                }
            }
        };
    }

    private static nint MakePointLParam(int x, int y)
    {
        return (nint)((y << 16) | (x & 0xFFFF));
    }
}
