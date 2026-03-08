using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class ClipboardRoute
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/clipboard", (HttpContext ctx) =>
        {
            var clipboard = ctx.RequestServices.GetRequiredService<ClipboardService>();
            var text = clipboard.GetText();
            return Results.Json(new ClipboardResponse(true, text), GeisterhandServer.JsonOptions);
        });

        app.MapPost("/clipboard", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<ClipboardWriteRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            var clipboard = ctx.RequestServices.GetRequiredService<ClipboardService>();
            clipboard.SetText(request.Text);
            return Results.Json(new ClipboardResponse(true, request.Text), GeisterhandServer.JsonOptions);
        });
    }
}
