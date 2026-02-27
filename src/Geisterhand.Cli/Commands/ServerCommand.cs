using System.CommandLine;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class ServerCommand
{
    public static Command Create()
    {
        var portOption = new Option<int>("--port") { Description = "Port to listen on", DefaultValueFactory = _ => 7676 };

        var command = new Command("server", "Start the HTTP API server");
        command.Add(portOption);

        command.SetAction(async (parseResult, ct) =>
        {
            int port = parseResult.GetValue(portOption);
            Console.WriteLine($"Starting Geisterhand server on http://127.0.0.1:{port}");

            var server = new GeisterhandServer(port);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await server.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Server stopped.");
            }
        });

        return command;
    }
}
