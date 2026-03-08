using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ClipboardCommand
{
    public static Command Create()
    {
        var actionArg = new Argument<string>("action") { Description = "Action: read or write" };
        var textArg = new Argument<string?>("text") { Description = "Text to write (for write action)", DefaultValueFactory = _ => null };

        var command = new Command("clipboard", "Read or write clipboard text");
        command.Add(actionArg);
        command.Add(textArg);

        command.SetAction((parseResult) =>
        {
            string action = parseResult.GetValue(actionArg)!;
            string? text = parseResult.GetValue(textArg);

            var clipboard = new ClipboardService();

            switch (action.ToLowerInvariant())
            {
                case "read":
                    var readText = clipboard.GetText();
                    var readResponse = new ClipboardResponse(true, readText);
                    Console.WriteLine(JsonSerializer.Serialize(readResponse, GeisterhandServer.JsonOptions));
                    break;

                case "write":
                    if (string.IsNullOrEmpty(text))
                    {
                        Console.Error.WriteLine("Text argument required for write action");
                        return;
                    }
                    clipboard.SetText(text);
                    var writeResponse = new ClipboardResponse(true, text);
                    Console.WriteLine(JsonSerializer.Serialize(writeResponse, GeisterhandServer.JsonOptions));
                    break;

                default:
                    Console.Error.WriteLine($"Unknown action: {action}. Use 'read' or 'write'.");
                    break;
            }
        });

        return command;
    }
}
