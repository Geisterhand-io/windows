using System.Diagnostics;
using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Permissions;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class StatusRoute
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/status", (HttpContext ctx) =>
        {
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var windows = capture.GetVisibleWindows();

            var runningApps = windows
                .GroupBy(w => w.ProcessId)
                .Select(g =>
                {
                    var first = g.First();
                    var foregroundHwnd = Geisterhand.Core.Native.User32.GetForegroundWindow();
                    bool isActive = g.Any(w => w.Handle == foregroundHwnd);
                    return new RunningApplication(
                        Name: first.ProcessName,
                        BundleIdentifier: first.ExecutablePath,
                        Pid: first.ProcessId,
                        IsActive: isActive
                    );
                })
                .ToList();

            var response = new StatusResponse(
                Status: "ok",
                Version: GeisterhandServer.Version,
                Platform: "windows",
                ApiVersion: GeisterhandServer.ApiVersion,
                Permissions: new PermissionsInfo(
                    Accessibility: PermissionManager.IsAccessibilityGranted,
                    ScreenRecording: PermissionManager.IsScreenRecordingGranted
                ),
                RunningApplications: runningApps
            );

            return Results.Json(response, GeisterhandServer.JsonOptions);
        });
    }
}
