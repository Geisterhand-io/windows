using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Geisterhand.Core.Server.Routes;

public static class MonitorRoute
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/monitors", (HttpContext ctx) =>
        {
            var monitorService = ctx.RequestServices.GetRequiredService<MonitorService>();
            var monitors = monitorService.GetMonitors();
            return Results.Json(new MonitorsResponse(monitors), GeisterhandServer.JsonOptions);
        });
    }
}
