using System.Drawing;
using System.Runtime.InteropServices;
using Geisterhand.Core.Native;

namespace Geisterhand.Core.Accessibility;

/// <summary>
/// Shows a colored border overlay around an element for visual debugging.
/// Uses a transparent layered window via Win32 API (no WinForms dependency).
/// </summary>
public static class HighlightOverlay
{
    private const string ClassName = "GeisterhandHighlight";
    private static bool s_classRegistered;

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public IntPtr lpszMenuName;
        public IntPtr lpszClassName;
        public IntPtr hIconSm;
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private static WndProcDelegate? s_wndProc; // prevent GC

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassExW(ref WNDCLASSEXW lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandleW(IntPtr lpModuleName);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreatePen(int iStyle, int cWidth, uint color);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr ho);

    [DllImport("gdi32.dll")]
    private static extern IntPtr GetStockObject(int i);

    [DllImport("gdi32.dll")]
    private static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public User32.RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint LWA_COLORKEY = 0x00000001;
    private const int NULL_BRUSH = 5;
    private const uint WM_PAINT = 0x000F;
    private const uint WM_DESTROY = 0x0002;

    private static uint ColorToRgb(Color c) => (uint)(c.R | (c.G << 8) | (c.B << 16));

    public static void Show(int x, int y, int width, int height, int durationMs = 2000, string color = "red")
    {
        var borderColor = color.ToLowerInvariant() switch
        {
            "green" => Color.Lime,
            "blue" => Color.Blue,
            "yellow" => Color.Yellow,
            "orange" => Color.Orange,
            "magenta" => Color.Magenta,
            _ => Color.Red
        };

        int borderWidth = 3;
        int pad = borderWidth;

        var thread = new Thread(() =>
        {
            var hInstance = GetModuleHandleW(IntPtr.Zero);

            if (!s_classRegistered)
            {
                s_wndProc = (hWnd, msg, wParam, lParam) =>
                {
                    return DefWindowProcW(hWnd, msg, wParam, lParam);
                };

                var wc = new WNDCLASSEXW
                {
                    cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(s_wndProc),
                    hInstance = hInstance,
                    lpszClassName = Marshal.StringToHGlobalUni(ClassName),
                    hbrBackground = IntPtr.Zero
                };
                RegisterClassExW(ref wc);
                s_classRegistered = true;
            }

            var hwnd = CreateWindowExW(
                WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
                ClassName, "",
                WS_POPUP | WS_VISIBLE,
                x - pad, y - pad, width + pad * 2, height + pad * 2,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            // Set black as transparent color key
            SetLayeredWindowAttributes(hwnd, 0x00000000, 255, LWA_COLORKEY);

            // Paint the border
            var hdc = User32.GetDC(hwnd);
            var pen = CreatePen(0, borderWidth, ColorToRgb(borderColor));
            var oldPen = SelectObject(hdc, pen);
            var nullBrush = GetStockObject(NULL_BRUSH);
            var oldBrush = SelectObject(hdc, nullBrush);
            Rectangle(hdc, 0, 0, width + pad * 2, height + pad * 2);
            SelectObject(hdc, oldPen);
            SelectObject(hdc, oldBrush);
            DeleteObject(pen);
            User32.ReleaseDC(hwnd, hdc);

            Thread.Sleep(durationMs);
            DestroyWindow(hwnd);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }
}
