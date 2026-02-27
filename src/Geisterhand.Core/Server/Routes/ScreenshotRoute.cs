using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class ScreenshotRoute
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/screenshot", async (HttpContext ctx) =>
        {
            var capture = ctx.RequestServices.GetRequiredService<ScreenCaptureService>();
            var serverCtx = ctx.RequestServices.GetRequiredService<ServerContext>();

            string? appName = ctx.Request.Query["app_name"].FirstOrDefault() ?? serverCtx.TargetAppName;
            string? pidStr = ctx.Request.Query["pid"].FirstOrDefault();
            int? pid = pidStr != null ? int.Parse(pidStr) : serverCtx.TargetPid;
            string format = ctx.Request.Query["format"].FirstOrDefault() ?? "png";
            string? qualityStr = ctx.Request.Query["quality"].FirstOrDefault();
            int quality = qualityStr != null ? int.Parse(qualityStr) : 85;

            System.Drawing.Bitmap bitmap;
            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                await Task.Delay(100); // brief pause for window to come to front
                bitmap = capture.CaptureWindow(hWnd);
            }
            else
            {
                bitmap = capture.CaptureScreen();
            }

            using (bitmap)
            {
                string base64 = ImageEncoder.EncodeToBase64(bitmap, format, quality);
                var response = new ScreenshotResponse(
                    Image: base64,
                    Format: format,
                    Width: bitmap.Width,
                    Height: bitmap.Height
                );
                return Results.Json(response, GeisterhandServer.JsonOptions);
            }
        });
    }
}
