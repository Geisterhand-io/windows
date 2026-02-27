using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ScrollCommand
{
    public static Command Create()
    {
        var xArg = new Argument<int>("x") { Description = "X coordinate" };
        var yArg = new Argument<int>("y") { Description = "Y coordinate" };
        var deltaXOption = new Option<int>("--delta-x") { Description = "Horizontal scroll amount", DefaultValueFactory = _ => 0 };
        var deltaYOption = new Option<int>("--delta-y") { Description = "Vertical scroll amount", DefaultValueFactory = _ => 0 };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("scroll", "Scroll at screen coordinates");
        command.Add(xArg);
        command.Add(yArg);
        command.Add(deltaXOption);
        command.Add(deltaYOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            int x = parseResult.GetValue(xArg);
            int y = parseResult.GetValue(yArg);
            int deltaX = parseResult.GetValue(deltaXOption);
            int deltaY = parseResult.GetValue(deltaYOption);
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);

            var mouse = new MouseController();
            var capture = new ScreenCaptureService();

            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                Thread.Sleep(50);
            }

            mouse.Scroll(x, y, deltaX, deltaY);

            var response = new ScrollResponse(true, x, y, deltaX, deltaY);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
