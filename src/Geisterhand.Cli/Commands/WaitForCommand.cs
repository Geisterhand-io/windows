using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Accessibility;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class WaitForCommand
{
    public static Command Create()
    {
        var roleOption = new Option<string?>("--role") { Description = "Element role to match" };
        var titleOption = new Option<string?>("--title") { Description = "Exact title to match" };
        var titleContainsOption = new Option<string?>("--title-contains") { Description = "Partial title match" };
        var valueOption = new Option<string?>("--value") { Description = "Element value to match" };
        var timeoutOption = new Option<int>("--timeout") { Description = "Timeout in seconds", DefaultValueFactory = _ => 10 };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("wait-for", "Wait for a UI element to appear");
        command.Add(roleOption);
        command.Add(titleOption);
        command.Add(titleContainsOption);
        command.Add(valueOption);
        command.Add(timeoutOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction(async (parseResult) =>
        {
            string? role = parseResult.GetValue(roleOption);
            string? title = parseResult.GetValue(titleOption);
            string? titleContains = parseResult.GetValue(titleContainsOption);
            string? value = parseResult.GetValue(valueOption);
            int timeout = parseResult.GetValue(timeoutOption);
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);

            var axService = new AccessibilityService();
            var capture = new ScreenCaptureService();
            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var results = await axService.WaitForElement(
                rootElement, role, title, titleContains, value,
                timeout * 1000, 250, 10);

            if (results.Count == 0)
            {
                Console.Error.WriteLine("Timeout: element not found");
                Environment.ExitCode = 1;
                return;
            }

            var response = new AccessibilitySearchResponse(results, results.Count);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
