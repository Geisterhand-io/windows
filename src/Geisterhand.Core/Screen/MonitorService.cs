using Geisterhand.Core.Models;
using Geisterhand.Core.Native;

namespace Geisterhand.Core.Screen;

public class MonitorService
{
    public List<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();

        User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref User32.RECT lprcMonitor, IntPtr dwData) =>
        {
            var info = new User32.MONITORINFOEX();
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<User32.MONITORINFOEX>();
            info.szDevice = new char[32];

            if (User32.GetMonitorInfo(hMonitor, ref info))
            {
                bool isPrimary = (info.dwFlags & User32.MONITORINFOF_PRIMARY) != 0;
                int width = info.rcMonitor.Right - info.rcMonitor.Left;
                int height = info.rcMonitor.Bottom - info.rcMonitor.Top;

                int dpi = 96;
                try
                {
                    if (User32.GetDpiForMonitor(hMonitor, 0, out uint dpiX, out uint _) == 0)
                        dpi = (int)dpiX;
                }
                catch { }

                string name = new string(info.szDevice).TrimEnd('\0');

                monitors.Add(new MonitorInfo(
                    Name: name,
                    X: info.rcMonitor.Left,
                    Y: info.rcMonitor.Top,
                    Width: width,
                    Height: height,
                    Dpi: dpi,
                    IsPrimary: isPrimary
                ));
            }
            return true;
        }, IntPtr.Zero);

        return monitors;
    }
}
