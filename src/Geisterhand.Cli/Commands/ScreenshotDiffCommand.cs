using System.CommandLine;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ScreenshotDiffCommand
{
    public static Command Create()
    {
        var baselineArg = new Argument<string>("baseline") { Description = "Path to baseline image" };
        var currentArg = new Argument<string>("current") { Description = "Path to current image" };
        var thresholdOption = new Option<double>("--threshold") { Description = "Diff threshold (0.0-1.0)", DefaultValueFactory = _ => 0.01 };
        var outputOption = new Option<string?>("--output") { Description = "Output path for diff image" };

        var command = new Command("screenshot-diff", "Compare two screenshots and report differences");
        command.Add(baselineArg);
        command.Add(currentArg);
        command.Add(thresholdOption);
        command.Add(outputOption);

        command.SetAction((parseResult) =>
        {
            string baselinePath = parseResult.GetValue(baselineArg)!;
            string currentPath = parseResult.GetValue(currentArg)!;
            double threshold = parseResult.GetValue(thresholdOption);
            string? outputPath = parseResult.GetValue(outputOption);

            using var baseline = new Bitmap(baselinePath);
            using var current = new Bitmap(currentPath);

            var diffService = new ImageDiffService();
            var (match, diffPercent, diffImage) = diffService.Compare(baseline, current, threshold);

            if (diffImage != null && !string.IsNullOrEmpty(outputPath))
            {
                diffImage.Save(outputPath, ImageFormat.Png);
            }
            diffImage?.Dispose();

            var result = new { match, diff_percent = diffPercent, output = outputPath };
            Console.WriteLine(JsonSerializer.Serialize(result, GeisterhandServer.JsonOptions));

            if (!match) Environment.ExitCode = 1;
        });

        return command;
    }
}
