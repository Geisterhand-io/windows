using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class WindowRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/window", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<WindowManageRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var windowMgr = ctx.RequestServices.GetRequiredService<WindowManager>();
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = request.AppName ?? serverCtx.TargetAppName;
            int? pid = request.Pid ?? serverCtx.TargetPid;

            var hWnd = capture.ResolveWindow(appName, pid);

            switch (request.Action.ToLowerInvariant())
            {
                case "resize":
                    if (request.Width == null || request.Height == null)
                        return Results.Json(new ErrorResponse("invalid_request", "width and height required for resize"), GeisterhandServer.JsonOptions, statusCode: 400);
                    windowMgr.Resize(hWnd, request.Width.Value, request.Height.Value);
                    return Results.Json(new WindowManageResponse(true, "resize", Width: request.Width, Height: request.Height), GeisterhandServer.JsonOptions);

                case "move":
                    if (request.X == null || request.Y == null)
                        return Results.Json(new ErrorResponse("invalid_request", "x and y required for move"), GeisterhandServer.JsonOptions, statusCode: 400);
                    windowMgr.Move(hWnd, request.X.Value, request.Y.Value);
                    return Results.Json(new WindowManageResponse(true, "move", X: request.X, Y: request.Y), GeisterhandServer.JsonOptions);

                case "maximize":
                    windowMgr.Maximize(hWnd);
                    return Results.Json(new WindowManageResponse(true, "maximize"), GeisterhandServer.JsonOptions);

                case "minimize":
                    windowMgr.Minimize(hWnd);
                    return Results.Json(new WindowManageResponse(true, "minimize"), GeisterhandServer.JsonOptions);

                case "restore":
                    windowMgr.Restore(hWnd);
                    return Results.Json(new WindowManageResponse(true, "restore"), GeisterhandServer.JsonOptions);

                case "close":
                    windowMgr.Close(hWnd);
                    return Results.Json(new WindowManageResponse(true, "close"), GeisterhandServer.JsonOptions);

                case "get-rect":
                case "get_rect":
                    var (x, y, w, h) = windowMgr.GetRect(hWnd);
                    return Results.Json(new WindowManageResponse(true, "get-rect", X: x, Y: y, Width: w, Height: h), GeisterhandServer.JsonOptions);

                case "get-state":
                case "get_state":
                    var state = windowMgr.GetState(hWnd);
                    return Results.Json(new WindowManageResponse(true, "get-state", State: state), GeisterhandServer.JsonOptions);

                default:
                    return Results.Json(new ErrorResponse("invalid_request", $"Unknown action: {request.Action}"), GeisterhandServer.JsonOptions, statusCode: 400);
            }
        });
    }
}
