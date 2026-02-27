using System.CommandLine;
using System.IO;
using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ScreenshotCommand
{
    public static Command Create()
    {
        var appNameOption = new Option<string?>("--app-name") { Description = "Target application name" };
        var pidOption = new Option<int?>("--pid") { Description = "Target process ID" };
        var formatOption = new Option<string>("--format") { Description = "Image format (png, jpeg)", DefaultValueFactory = _ => "png" };
        var qualityOption = new Option<int>("--quality") { Description = "JPEG quality (1-100)", DefaultValueFactory = _ => 85 };
        var outputOption = new Option<string?>("--output") { Description = "Save to file path instead of base64 JSON" };

        var command = new Command("screenshot", "Take a screenshot");
        command.Add(appNameOption);
        command.Add(pidOption);
        command.Add(formatOption);
        command.Add(qualityOption);
        command.Add(outputOption);

        command.SetAction((parseResult) =>
        {
            string? appName = parseResult.GetValue(appNameOption);
            int? pid = parseResult.GetValue(pidOption);
            string format = parseResult.GetValue(formatOption)!;
            int quality = parseResult.GetValue(qualityOption);
            string? output = parseResult.GetValue(outputOption);

            var capture = new ScreenCaptureService();

            System.Drawing.Bitmap bitmap;
            if (pid.HasValue || !string.IsNullOrEmpty(appName))
            {
                var hWnd = capture.ResolveWindow(appName, pid);
                capture.BringWindowToFront(hWnd);
                Thread.Sleep(100);
                bitmap = capture.CaptureWindow(hWnd);
            }
            else
            {
                bitmap = capture.CaptureScreen();
            }

            using (bitmap)
            {
                if (!string.IsNullOrEmpty(output))
                {
                    var bytes = ImageEncoder.Encode(bitmap, format, quality);
                    File.WriteAllBytes(output, bytes);
                    Console.WriteLine($"Screenshot saved to {output}");
                }
                else
                {
                    string base64 = ImageEncoder.EncodeToBase64(bitmap, format, quality);
                    var response = new ScreenshotResponse(base64, format, bitmap.Width, bitmap.Height);
                    Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
                }
            }
        });

        return command;
    }
}
