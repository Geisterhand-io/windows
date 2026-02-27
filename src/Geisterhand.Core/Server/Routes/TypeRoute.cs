using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class TypeRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/type", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<TypeRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var keyboard = ctx.RequestServices.GetRequiredService<KeyboardController>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;
            string method = "sendInput";

            int charCount;
            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                await Task.Delay(50);

                if (request.UseClipboard)
                {
                    // Use clipboard paste
                    SetClipboardText(request.Text);
                    keyboard.PressKey("v", ["ctrl"]);
                    charCount = request.Text.Length;
                    method = "clipboard";
                }
                else
                {
                    charCount = keyboard.TypeText(request.Text);
                    method = "sendInput";
                }
            }
            else
            {
                if (request.UseClipboard)
                {
                    SetClipboardText(request.Text);
                    keyboard.PressKey("v", ["ctrl"]);
                    charCount = request.Text.Length;
                    method = "clipboard";
                }
                else
                {
                    charCount = keyboard.TypeText(request.Text);
                }
            }

            var response = new TypeResponse(
                Success: true,
                CharactersTyped: charCount,
                Method: method
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool CloseClipboard();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool EmptyClipboard();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;

    private static void SetClipboardText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero)) return;
        try
        {
            EmptyClipboard();
            var bytes = (text.Length + 1) * 2;
            var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);
            if (hGlobal == IntPtr.Zero) return;
            var ptr = GlobalLock(hGlobal);
            System.Runtime.InteropServices.Marshal.Copy(text.ToCharArray(), 0, ptr, text.Length);
            // null terminator is already zero from GlobalAlloc
            GlobalUnlock(hGlobal);
            SetClipboardData(CF_UNICODETEXT, hGlobal);
        }
        finally
        {
            CloseClipboard();
        }
    }
}
