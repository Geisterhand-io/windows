using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Accessibility;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class HighlightCommand
{
    public static Command Create()
    {
        var pathOption = new Option<string?>("--path") { Description = "Element path as comma-separated indices (e.g., 0,1,3)" };
        var durationOption = new Option<int>("--duration") { Description = "Duration in seconds", DefaultValueFactory = _ => 2 };
        var colorOption = new Option<string>("--color") { Description = "Border color (red, green, blue, yellow)", DefaultValueFactory = _ => "red" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };

        var command = new Command("highlight", "Highlight a UI element with a colored border");
        command.Add(pathOption);
        command.Add(durationOption);
        command.Add(colorOption);
        command.Add(appNameOption);
        command.Add(pidOption);

        command.SetAction((parseResult) =>
        {
            string? pathStr = parseResult.GetValue(pathOption);
            int duration = parseResult.GetValue(durationOption);
            string color = parseResult.GetValue(colorOption)!;
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);

            var axService = new AccessibilityService();
            var capture = new ScreenCaptureService();
            var hWnd = capture.ResolveWindow(appName, pid);
            var rootElement = axService.GetWindowElement(hWnd);

            var targetElement = rootElement;
            if (!string.IsNullOrEmpty(pathStr))
            {
                var path = pathStr.Split(',').Select(int.Parse).ToList();
                targetElement = axService.NavigateToPath(rootElement, path);
            }

            var rect = targetElement.Current.BoundingRectangle;
            if (rect.IsEmpty)
            {
                Console.Error.WriteLine("Element has no bounding rectangle");
                Environment.ExitCode = 1;
                return;
            }

            HighlightOverlay.Show((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, duration * 1000, color);

            // Wait for highlight to finish
            Thread.Sleep(duration * 1000 + 100);

            Console.WriteLine(JsonSerializer.Serialize(new { success = true }, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
