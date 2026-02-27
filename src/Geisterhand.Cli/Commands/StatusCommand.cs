using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Permissions;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class StatusCommand
{
    public static Command Create()
    {
        var portOption = new Option<int>("--port") { Description = "Server port", DefaultValueFactory = _ => 7676 };

        var command = new Command("status", "Show system status and running applications");
        command.Add(portOption);

        command.SetAction(async (parseResult, ct) =>
        {
            int port = parseResult.GetValue(portOption);

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync($"http://127.0.0.1:{port}/status", ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    Console.WriteLine(json);
                    return;
                }
            }
            catch { }

            var capture = new ScreenCaptureService();
            var windows = capture.GetVisibleWindows();

            var runningApps = windows
                .GroupBy(w => w.ProcessId)
                .Select(g =>
                {
                    var first = g.First();
                    return new RunningApplication(
                        Name: first.ProcessName,
                        BundleIdentifier: first.ExecutablePath,
                        Pid: first.ProcessId,
                        IsActive: false
                    );
                })
                .ToList();

            var status = new StatusResponse(
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

            Console.WriteLine(JsonSerializer.Serialize(status, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
