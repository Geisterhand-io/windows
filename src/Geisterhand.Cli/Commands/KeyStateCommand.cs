using System.CommandLine;
using System.Text.Json;
using Geisterhand.Core.Input;
using Geisterhand.Core.Models;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class KeyStateCommand
{
    public static Command Create()
    {
        var keyArg = new Argument<string>("key") { Description = "Key name (e.g., capslock, shift, ctrl)" };

        var command = new Command("key-state", "Query the state of a key (pressed/toggled)");
        command.Add(keyArg);

        command.SetAction((parseResult) =>
        {
            string keyName = parseResult.GetValue(keyArg)!;

            var keyboard = new KeyboardController();
            var (pressed, toggled) = keyboard.GetKeyState(keyName);

            var response = new KeyStateResponse(keyName, pressed, toggled);
            Console.WriteLine(JsonSerializer.Serialize(response, GeisterhandServer.JsonOptions));
        });

        return command;
    }
}
