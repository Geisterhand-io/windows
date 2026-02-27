using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class KeyCommand
{
    public static Command Create()
    {
        var keyArg = new Argument<string>("key") { Description = "Key name (e.g., return, tab, a, f1)" };
        var modifiersOption = new Option<string[]>("--modifiers") { Description = "Modifier keys (e.g., cmd, ctrl, alt, shift)" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("key", "Press a key with optional modifiers");
        command.Add(keyArg);
        command.Add(modifiersOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            string key = parseResult.GetValue(keyArg)!;
            string[]? modifiers = parseResult.GetValue(modifiersOption);
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

            var modList = modifiers?.ToList() ?? [];
            keyboard.PressKey(key, modList);

            var response = new KeyResponse(true, key, modList);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
