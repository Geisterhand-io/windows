using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ClickCommand
{
    public static Command Create()
    {
        var xArg = new Argument<int>("x") { Description = "X coordinate" };
        var yArg = new Argument<int>("y") { Description = "Y coordinate" };
        var buttonOption = new Option<string>("--button") { Description = "Mouse button (left, right, middle)", DefaultValueFactory = _ => "left" };
        var clickTypeOption = new Option<string>("--click-type") { Description = "Click type (single, double, triple)", DefaultValueFactory = _ => "single" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("click", "Click at screen coordinates");
        command.Add(xArg);
        command.Add(yArg);
        command.Add(buttonOption);
        command.Add(clickTypeOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            int x = parseResult.GetValue(xArg);
            int y = parseResult.GetValue(yArg);
            string button = parseResult.GetValue(buttonOption)!;
            string clickType = parseResult.GetValue(clickTypeOption)!;
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

            mouse.Click(x, y, button, clickType);

            var response = new ClickResponse(true, x, y, button, clickType);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
