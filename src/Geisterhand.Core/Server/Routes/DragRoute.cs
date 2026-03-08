using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class DragRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/drag", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<DragRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var mouse = ctx.RequestServices.GetRequiredService<MouseController>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                await Task.Delay(50);
            }

            mouse.Drag(request.StartX, request.StartY, request.EndX, request.EndY, request.DurationMs, request.Button);

            return Results.Json(new DragResponse(true, request.StartX, request.StartY, request.EndX, request.EndY), GeisterhandServer.JsonOptions);
        });

        app.MapPost("/select-text", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<SelectTextRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var mouse = ctx.RequestServices.GetRequiredService<MouseController>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                await Task.Delay(50);
            }

            mouse.SelectRange(request.StartX, request.StartY, request.EndX, request.EndY);

            return Results.Json(new { success = true }, GeisterhandServer.JsonOptions);
        });
    }
}
