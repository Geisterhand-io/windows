using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class TypeCommand
{
    public static Command Create()
    {
        var textArg = new Argument<string>("text") { Description = "Text to type" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("type", "Type text");
        command.Add(textArg);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            string text = parseResult.GetValue(textArg)!;
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);

            var keyboard = new KeyboardController();
            var capture = new ScreenCaptureService();

            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                Thread.Sleep(50);
            }

            int count = keyboard.TypeText(text);

            var response = new TypeResponse(true, count, "sendInput");
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
