using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class WindowCommand
{
    public static Command Create()
    {
        var actionArg = new Argument<string>("action") { Description = "Action: resize, move, maximize, minimize, restore, close, get-rect, get-state" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };
        var xOption = new Option<int?>("--x") { Description = "X position" };
        var yOption = new Option<int?>("--y") { Description = "Y position" };
        var widthOption = new Option<int?>("--width") { Description = "Window width" };
        var heightOption = new Option<int?>("--height") { Description = "Window height" };

        var command = new Command("window", "Manage window (resize, move, maximize, minimize, restore, close)");
        command.Add(actionArg);
        command.Add(appNameOption);
        command.Add(pidOption);
        command.Add(xOption);
        command.Add(yOption);
        command.Add(widthOption);
        command.Add(heightOption);

        command.SetAction((parseResult) =>
        {
            string action = parseResult.GetValue(actionArg)!;
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);
            int? x = parseResult.GetValue(xOption);
            int? y = parseResult.GetValue(yOption);
            int? width = parseResult.GetValue(widthOption);
            int? height = parseResult.GetValue(heightOption);

            var windowMgr = new WindowManager();
            var capture = new ScreenCaptureService();
            var hWnd = capture.ResolveWindow(appName, pid);

            WindowManageResponse response;
            switch (action.ToLowerInvariant())
            {
                case "resize":
                    windowMgr.Resize(hWnd, width ?? 800, height ?? 600);
                    response = new WindowManageResponse(true, "resize", Width: width, Height: height);
                    break;
                case "move":
                    windowMgr.Move(hWnd, x ?? 0, y ?? 0);
                    response = new WindowManageResponse(true, "move", X: x, Y: y);
                    break;
                case "maximize":
                    windowMgr.Maximize(hWnd);
                    response = new WindowManageResponse(true, "maximize");
                    break;
                case "minimize":
                    windowMgr.Minimize(hWnd);
                    response = new WindowManageResponse(true, "minimize");
                    break;
                case "restore":
                    windowMgr.Restore(hWnd);
                    response = new WindowManageResponse(true, "restore");
                    break;
                case "close":
                    windowMgr.Close(hWnd);
                    response = new WindowManageResponse(true, "close");
                    break;
                case "get-rect":
                case "get_rect":
                    var (rx, ry, rw, rh) = windowMgr.GetRect(hWnd);
                    response = new WindowManageResponse(true, "get-rect", X: rx, Y: ry, Width: rw, Height: rh);
                    break;
                case "get-state":
                case "get_state":
                    var state = windowMgr.GetState(hWnd);
                    response = new WindowManageResponse(true, "get-state", State: state);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown action: {action}");
                    return;
            }

            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
