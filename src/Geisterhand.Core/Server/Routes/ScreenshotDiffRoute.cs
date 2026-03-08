using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class ScreenshotDiffRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/screenshot/diff", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<ScreenshotDiffRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var diffService = ctx.RequestServices.GetRequiredService<ImageDiffService>();

            // Decode base64 images
            byte[] baselineBytes = Convert.FromBase64String(request.Baseline);
            byte[] currentBytes = Convert.FromBase64String(request.Current);

            using var baselineStream = new MemoryStream(baselineBytes);
            using var currentStream = new MemoryStream(currentBytes);
            using var baseline = new Bitmap(baselineStream);
            using var current = new Bitmap(currentStream);

            var (match, diffPercent, diffImage) = diffService.Compare(baseline, current, request.Threshold);

            string? diffBase64 = null;
            if (diffImage != null)
            {
                using var ms = new MemoryStream();
                diffImage.Save(ms, ImageFormat.Png);
                diffBase64 = Convert.ToBase64String(ms.ToArray());
                diffImage.Dispose();
            }

            return Results.Json(new ScreenshotDiffResponse(match, diffPercent, diffBase64), GeisterhandServer.JsonOptions);
        });
    }
}
