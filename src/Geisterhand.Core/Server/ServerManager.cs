namespace Geisterhand.Core.Server;

public class ServerManager
{
    private GeisterhandServer? _server;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private readonly object _lock = new();

    public bool IsRunning { get; private set; }
    public int Port { get; private set; }

    public async Task StartAsync(int port = 7676, int? targetPid = null, string? targetAppName = null)
    {
        lock (_lock)
        {
            if (IsRunning) return;
        }

        _cts = new CancellationTokenSource();
        _server = new GeisterhandServer(port, targetPid, targetAppName);
        Port = port;

        await _server.StartAsync(_cts.Token);
        IsRunning = true;

        // Keep the server running in background
        _serverTask = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Timeout.Infinite, _cts.Token);
            }
            catch (OperationCanceledException) { }
        });
    }

    public async Task StopAsync()
    {
        lock (_lock)
        {
            if (!IsRunning) return;
        }

        _cts?.Cancel();
        if (_server != null)
        {
            await _server.StopAsync();
        }
        if (_serverTask != null)
        {
            try { await _serverTask; } catch (OperationCanceledException) { }
        }

        IsRunning = false;
        _server = null;
        _cts = null;
        _serverTask = null;
    }

    public async Task RestartAsync(int port = 7676, int? targetPid = null, string? targetAppName = null)
    {
        await StopAsync();
        await StartAsync(port, targetPid, targetAppName);
    }

    /// <summary>
    /// Find an available port starting from the given port.
    /// </summary>
    public static int FindAvailablePort(int startPort = 7677)
    {
        for (int port = startPort; port < startPort + 100; port++)
        {
            try
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch { }
        }
        throw new InvalidOperationException("No available port found.");
    }
}
