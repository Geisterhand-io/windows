using System.Text.Json;
using Geisterhand.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Geisterhand.Core.Server.Routes;

public static class WaitRoute
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/wait", async (HttpContext ctx) =>
        {
            var request = await ctx.Request.ReadFromJsonAsync<WaitRequest>(GeisterhandServer.JsonOptions);
            if (request == null)
                return Results.Json(new ErrorResponse("invalid_request", "Missing request body"), GeisterhandServer.JsonOptions, statusCode: 400);

            await Task.Delay(TimeSpan.FromSeconds(request.Seconds));

            var response = new WaitResponse(
                Success: true,
                WaitedSeconds: request.Seconds
            );
            return Results.Json(response, GeisterhandServer.JsonOptions);
        });
    }
}
