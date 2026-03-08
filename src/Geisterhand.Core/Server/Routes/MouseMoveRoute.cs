using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class MouseMoveRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/mouse-move", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<MouseMoveRequest>(GeisterhandServer.JsonOptions);
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

            mouse.MoveTo(request.X, request.Y);

            return Results.Json(new MouseMoveResponse(true, request.X, request.Y), GeisterhandServer.JsonOptions);
        });

        app.MapPost("/hover", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<HoverRequest>(GeisterhandServer.JsonOptions);
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

            mouse.MoveTo(request.X, request.Y);
            await Task.Delay(request.HoverDurationMs);

            return Results.Json(new HoverResponse(true, request.X, request.Y, request.HoverDurationMs), GeisterhandServer.JsonOptions);
        });
    }
}
