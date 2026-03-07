using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ClickWindowCommand
{
    public static Command Create()
    {
        var xArg = new Argument<int>("x") { Description = "X coordinate (client-area relative, physical pixels)" };
        var yArg = new Argument<int>("y") { Description = "Y coordinate (client-area relative, physical pixels)" };
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };
        var dryRunOption = new Option<bool>("--dry-run") { Description = "Just report what would be clicked", DefaultValueFactory = _ => false };
        var listOption = new Option<bool>("--list") { Description = "List all child button HWNDs", DefaultValueFactory = _ => false };

        var command = new Command("click-window", "Click inside a window at client-area coordinates (DPI-aware)");
        command.Add(xArg);
        command.Add(yArg);
        command.Add(appNameOption);
        command.Add(pidOption);
        command.Add(dryRunOption);
        command.Add(listOption);

        command.SetAction((parseResult) =>
        {
            WindowClicker.EnsureDpiAware();

            int x = parseResult.GetValue(xArg);
            int y = parseResult.GetValue(yArg);
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);
            bool dryRun = parseResult.GetValue(dryRunOption);
            bool list = parseResult.GetValue(listOption);

            var capture = new ScreenCaptureService();
            var hWnd = capture.ResolveWindow(appName, pid);
            var clicker = new WindowClicker();

            if (list)
            {
                var children = clicker.EnumChildren(hWnd);
                foreach (var c in children)
                {
                    if (c.ClassName.Equals("Button", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"hwnd=0x{c.Hwnd:X}  ctrlId={c.CtrlId}  text=\"{c.Text}\"");
                    }
                }
                return;
            }

            var target = clicker.FindTargetAt(hWnd, x, y);

            Console.Error.WriteLine($"Target: hwnd=0x{target.Hwnd:X} class=\"{target.ClassName}\" text=\"{target.Text}\" ctrlId={target.CtrlId}");

            if (dryRun)
            {
                Console.WriteLine(JsonSerializer.Serialize(new
                {
                    dryRun = true,
                    hwnd = $"0x{target.Hwnd:X}",
                    className = target.ClassName,
                    text = target.Text,
                    ctrlId = target.CtrlId,
                    x, y
                }, GeisterhandServer.JsonOptions));
                return;
            }

            clicker.Click(hWnd, target, x, y);

            Console.WriteLine(JsonSerializer.Serialize(new
            {
                success = true,
                hwnd = $"0x{target.Hwnd:X}",
                className = target.ClassName,
                text = target.Text,
                ctrlId = target.CtrlId,
                x, y
            }, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
