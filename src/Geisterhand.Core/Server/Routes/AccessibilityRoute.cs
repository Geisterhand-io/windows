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
            var tree = axService.BuildTree(element, maxDepth, format);

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
                ? axService.NavigateToPath(rootElement, request.Path)
                : rootElement;

            var (role, title) = axService.PerformAction(targetElement, request.Action, request.Value);

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

            var hWnd = capture.ResolveWindow(appName, pid);
            var element = axService.GetWindowElement(hWnd);

            var results = axService.Search(element, role, title, titleContains, value, maxResults, maxDepth);

            var response = new AccessibilitySearchResponse(
                Results: results,
                Count: results.Count
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });
    }
}
