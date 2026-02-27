using System.Text.Json;
using Geisterhand.Core.Accessibility;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Geisterhand.Core.Server;

public class GeisterhandServer
{
    public const string Version = "1.0.0";
    public const string ApiVersion = "1";

    private readonly int _port;
    private readonly int? _targetPid;
    private readonly string? _targetAppName;
    private WebApplication? _app;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public GeisterhandServer(int port = 7676, int? targetPid = null, string? targetAppName = null)
    {
        _port = port;
        _targetPid = targetPid;
        _targetAppName = targetAppName;
    }

    public int Port => _port;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(_port);
        });
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        // Register services
        builder.Services.AddSingleton(new KeyboardController());
        builder.Services.AddSingleton(new MouseController());
        builder.Services.AddSingleton(new ScreenCaptureService());
        builder.Services.AddSingleton(new AccessibilityService());
        builder.Services.AddSingleton(new MenuService());
        builder.Services.AddSingleton(new ServerContext(_targetPid, _targetAppName));

        _app = builder.Build();

        // Error handling middleware
        _app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var error = new ErrorResponse(Error: "internal_error", Detail: ex.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
            }
        });

        // Register routes
        StatusRoute.Map(_app);
        ScreenshotRoute.Map(_app);
        ClickRoute.Map(_app);
        TypeRoute.Map(_app);
        KeyRoute.Map(_app);
        ScrollRoute.Map(_app);
        WaitRoute.Map(_app);
        AccessibilityRoute.Map(_app);
        MenuRoute.Map(_app);

        await _app.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);
        await _app!.WaitForShutdownAsync(cancellationToken);
    }
}

/// <summary>
/// Holds the optional target PID/app name for scoped server instances (run command).
/// </summary>
public record ServerContext(int? TargetPid, string? TargetAppName);
