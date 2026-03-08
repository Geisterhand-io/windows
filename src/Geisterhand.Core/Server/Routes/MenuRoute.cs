using System.Text.Json;
using Geisterhand.Core.Accessibility;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class MenuRoute
{
    public static void Map(WebApplication app)
    {
        // GET /menu/list
        app.MapGet("/menu/list", (HttpContext ctx) =>
        {
            var menuService = ctx.RequestServices.GetRequiredService<MenuService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = ctx.Request.Query["app_name"].FirstOrDefault() ?? serverCtx.TargetAppName;
            string? pidStr = ctx.Request.Query["pid"].FirstOrDefault();
            int? pid = pidStr != null ? int.Parse(pidStr) : serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var windowElement = axService.GetWindowElement(hWnd);

            var menus = menuService.GetMenuItems(windowElement);

            Native.User32.GetWindowThreadProcessId(hWnd, out uint windowPid);
            string resolvedAppName = appName ?? System.Diagnostics.Process.GetProcessById((int)windowPid).ProcessName;

            var response = new MenuListResponse(
                AppName: resolvedAppName,
                Pid: (int)windowPid,
                Menus: menus
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });

        // POST /menu/trigger
        app.MapPost("/menu/trigger", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<MenuTriggerRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var menuService = ctx.RequestServices.GetRequiredService<MenuService>();
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            capture.BringWindowToFront(hWnd);
            await Task.Delay(50);

            var windowElement = axService.GetWindowElement(hWnd);
            menuService.TriggerMenuItem(windowElement, request.Path);

            var response = new MenuTriggerResponse(
                Success: true,
                MenuPath: request.Path.ToList()
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });

        // POST /menu/context — right-click and inspect context menu
        app.MapPost("/menu/context", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<ContextMenuRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var menuService = ctx.RequestServices.GetRequiredService<MenuService>();
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var mouse = ctx.RequestServices.GetRequiredService<Input.MouseController>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            capture.BringWindowToFront(hWnd);
            await Task.Delay(50);

            // Right-click at coordinates
            mouse.Click(request.X, request.Y, "right", "single");
            await Task.Delay(300); // Wait for context menu to appear

            // Find the context menu via UIA
            var desktop = System.Windows.Automation.AutomationElement.RootElement;
            var menuCondition = new System.Windows.Automation.PropertyCondition(
                System.Windows.Automation.AutomationElement.ControlTypeProperty,
                System.Windows.Automation.ControlType.Menu);

            var contextMenu = desktop.FindFirst(System.Windows.Automation.TreeScope.Children, menuCondition);

            if (contextMenu == null)
            {
                return Results.Json(new ContextMenuResponse([]), GeisterhandServer.JsonOptions);
            }

            var items = menuService.GetMenuItems(contextMenu);

            // Press Escape to dismiss
            Native.User32.PostMessageW(hWnd, Native.User32.WM_KEYDOWN, 0x1B, 0);
            Native.User32.PostMessageW(hWnd, Native.User32.WM_KEYUP, 0x1B, 0);

            return Results.Json(new ContextMenuResponse(items), GeisterhandServer.JsonOptions);
        });
    }
}
