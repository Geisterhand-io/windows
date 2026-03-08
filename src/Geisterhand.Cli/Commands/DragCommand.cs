using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class DragCommand
{
    public static Command Create()
    {
        var x1Arg = new Argument<int>("x1") { Description = "Start X coordinate" };
        var y1Arg = new Argument<int>("y1") { Description = "Start Y coordinate" };
        var x2Arg = new Argument<int>("x2") { Description = "End X coordinate" };
        var y2Arg = new Argument<int>("y2") { Description = "End Y coordinate" };
        var durationOption = new Option<int>("--duration") { Description = "Duration in ms", DefaultValueFactory = _ => 500 };
        var buttonOption = new Option<string>("--button") { Description = "Mouse button", DefaultValueFactory = _ => "left" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("drag", "Drag from one position to another");
        command.Add(x1Arg);
        command.Add(y1Arg);
        command.Add(x2Arg);
        command.Add(y2Arg);
        command.Add(durationOption);
        command.Add(buttonOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            int x1 = parseResult.GetValue(x1Arg);
            int y1 = parseResult.GetValue(y1Arg);
            int x2 = parseResult.GetValue(x2Arg);
            int y2 = parseResult.GetValue(y2Arg);
            int duration = parseResult.GetValue(durationOption);
            string button = parseResult.GetValue(buttonOption)!;
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

            mouse.Drag(x1, y1, x2, y2, duration, button);

            var response = new DragResponse(true, x1, y1, x2, y2);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
