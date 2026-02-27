using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class ClickRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/click", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<ClickRequest>(GeisterhandServer.JsonOptions);
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

            mouse.Click(request.X, request.Y, request.Button, request.ClickType);

            var response = new ClickResponse(
                Success: true,
                X: request.X,
                Y: request.Y,
                Button: request.Button,
                ClickType: request.ClickType
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });
    }
}
