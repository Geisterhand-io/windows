using System.Text.Json;
using Geisterhand.Core.Accessibility;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class AccessibilityRoute
{
    public static void Map(WebApplication app)
    {
        // GET /accessibility/tree
        app.MapGet("/accessibility/tree", (HttpContext ctx) =>
        {
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = ctx.Request.Query["app_name"].FirstOrDefault() ?? serverCtx.TargetAppName;
            string? pidStr = ctx.Request.Query["pid"].FirstOrDefault();
            int? pid = pidStr != null ? int.Parse(pidStr) : serverCtx.TargetPid;
            string format = ctx.Request.Query["format"].FirstOrDefault() ?? "full";
            string? maxDepthStr = ctx.Request.Query["max_depth"].FirstOrDefault();
            int maxDepth = maxDepthStr != null ? int.Parse(maxDepthStr) : 10;

            var hWnd = capture.ResolveWindow(appName, pid);
            var element = axService.GetWindowElement(hWnd);
            var tree = RetryPolicy.Execute(() => axService.BuildTree(element, maxDepth, format));

            Native.User32.GetWindowThreadProcessId(hWnd, out uint windowPid);
            string resolvedAppName = appName ?? System.Diagnostics.Process.GetProcessById((int)windowPid).ProcessName;

            var response = new AccessibilityTreeResponse(
                AppName: resolvedAppName,
                Pid: (int)windowPid,
                Tree: tree
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });

        // POST /accessibility/action
        app.MapPost("/accessibility/action", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<AccessibilityActionRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var targetElement = request.Path != null && request.Path.Count > 0
                ? RetryPolicy.Execute(() => axService.NavigateToPath(rootElement, request.Path))
                : rootElement;

            var (role, title) = RetryPolicy.Execute(() => axService.PerformAction(targetElement, request.Action, request.Value));

            var response = new AccessibilityActionResponse(
                Success: true,
                Action: request.Action,
                ElementRole: role,
                ElementTitle: title
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });

        // GET /accessibility/search
        app.MapGet("/accessibility/search", (HttpContext ctx) =>
        {
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = ctx.Request.Query["app_name"].FirstOrDefault() ?? serverCtx.TargetAppName;
            string? pidStr = ctx.Request.Query["pid"].FirstOrDefault();
            int? pid = pidStr != null ? int.Parse(pidStr) : serverCtx.TargetPid;
            string? role = ctx.Request.Query["role"].FirstOrDefault();
            string? title = ctx.Request.Query["title"].FirstOrDefault();
            string? titleContains = ctx.Request.Query["title_contains"].FirstOrDefault();
            string? value = ctx.Request.Query["value"].FirstOrDefault();
            string? maxResultsStr = ctx.Request.Query["max_results"].FirstOrDefault();
            int maxResults = maxResultsStr != null ? int.Parse(maxResultsStr) : 50;
            string? maxDepthStr = ctx.Request.Query["max_depth"].FirstOrDefault();
            int maxDepth = maxDepthStr != null ? int.Parse(maxDepthStr) : 10;
            string? titleRegex = ctx.Request.Query["title_regex"].FirstOrDefault();
            string? valueRegex = ctx.Request.Query["value_regex"].FirstOrDefault();
            string? automationId = ctx.Request.Query["automation_id"].FirstOrDefault();
            string? enabledOnlyStr = ctx.Request.Query["enabled_only"].FirstOrDefault();
            bool? enabledOnly = enabledOnlyStr != null ? bool.Parse(enabledOnlyStr) : null;
            string? visibleOnlyStr = ctx.Request.Query["visible_only"].FirstOrDefault();
            bool? visibleOnly = visibleOnlyStr != null ? bool.Parse(visibleOnlyStr) : null;

            var hWnd = capture.ResolveWindow(appName, pid);
            var element = axService.GetWindowElement(hWnd);

            var results = RetryPolicy.Execute(() =>
                axService.Search(element, role, title, titleContains, value, maxResults, maxDepth,
                    titleRegex, valueRegex, automationId, enabledOnly, visibleOnly));

            var response = new AccessibilitySearchResponse(
                Results: results,
                Count: results.Count
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });

        // POST /accessibility/wait — wait for element to appear
        app.MapPost("/accessibility/wait", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<WaitForElementRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var results = await axService.WaitForElement(
                rootElement, request.Role, request.Title, request.TitleContains, request.Value,
                request.TimeoutMs, request.PollIntervalMs, request.MaxDepth);

            if (results.Count == 0)
            {
                return Results.Json(new ErrorResponse("timeout", "Element not found within timeout"), GeisterhandServer.JsonOptions, statusCode: 408);
            }

            return Results.Json(new AccessibilitySearchResponse(results, results.Count), GeisterhandServer.JsonOptions);
        });

        // POST /accessibility/wait-condition — wait for condition on element
        app.MapPost("/accessibility/wait-condition", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<WaitForConditionRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool met = await axService.WaitForCondition(
                rootElement, request.Path, request.Condition, request.Value,
                request.TimeoutMs, request.PollIntervalMs);

            if (!met)
            {
                return Results.Json(new ErrorResponse("timeout", $"Condition '{request.Condition}' not met within timeout"), GeisterhandServer.JsonOptions, statusCode: 408);
            }

            return Results.Json(new WaitForConditionResponse(true, request.Condition, sw.ElapsedMilliseconds), GeisterhandServer.JsonOptions);
        });

        // GET /accessibility/at-point — get element at screen coordinates
        app.MapGet("/accessibility/at-point", (HttpContext ctx) =>
        {
            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();

            string? xStr = ctx.Request.Query["x"].FirstOrDefault();
            string? yStr = ctx.Request.Query["y"].FirstOrDefault();
            if (xStr == null || yStr == null)
                return Results.Json(new ErrorResponse("invalid_request", "x and y parameters required"), GeisterhandServer.JsonOptions, statusCode: 400);

            int x = int.Parse(xStr);
            int y = int.Parse(yStr);

            var element = axService.GetElementAtPoint(x, y);

            return Results.Json(new ElementAtPointResponse(element, element != null), GeisterhandServer.JsonOptions);
        });

        // POST /accessibility/navigate — relative navigation
        app.MapPost("/accessibility/navigate", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<NavigateRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var targetElement = axService.NavigateToPath(rootElement, request.Path);
            var navResult = axService.NavigateRelative(targetElement, request.Direction);

            var current = navResult.Current;
            string role = RoleMap.ToAxRole(current.ControlType);
            var rect = current.BoundingRectangle;

            var resultElement = new AccessibilityElement(
                Role: role,
                Title: string.IsNullOrEmpty(current.Name) ? null : current.Name,
                Position: rect.IsEmpty ? null : new ElementPosition(rect.X, rect.Y),
                Size: rect.IsEmpty ? null : new ElementSize(rect.Width, rect.Height),
                IsEnabled: current.IsEnabled,
                AutomationId: string.IsNullOrEmpty(current.AutomationId) ? null : current.AutomationId,
                ClassName: string.IsNullOrEmpty(current.ClassName) ? null : current.ClassName,
                ControlType: current.ControlType.ProgrammaticName.Replace("ControlType.", ""),
                ProcessId: current.ProcessId
            );

            return Results.Json(resultElement, GeisterhandServer.JsonOptions);
        });

        // POST /accessibility/highlight — highlight element with red border
        app.MapPost("/accessibility/highlight", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<HighlightRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var axService = ctx.RequestServices.GetRequiredService<AccessibilityService>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var targetElement = request.Path != null && request.Path.Count > 0
                ? axService.NavigateToPath(rootElement, request.Path)
                : rootElement;

            var rect = targetElement.Current.BoundingRectangle;
            if (rect.IsEmpty)
                return Results.Json(new ErrorResponse("element_error", "Element has no bounding rectangle"), GeisterhandServer.JsonOptions, statusCode: 400);

            // Show highlight overlay
            HighlightOverlay.Show((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, request.DurationMs, request.Color);

            return Results.Json(new HighlightResponse(true), GeisterhandServer.JsonOptions);
        });
    }
}
