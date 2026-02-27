using Geisterhand.Core.Server;

namespace Geisterhand.Tray;

public class StatusMonitor : IDisposable
{
    private readonly ServerManager _serverManager;
    private readonly Action<bool> _onStatusChanged;
    private readonly System.Threading.Timer _timer;
    private bool _lastStatus;

    public StatusMonitor(ServerManager serverManager, Action<bool> onStatusChanged)
    {
        _serverManager = serverManager;
        _onStatusChanged = onStatusChanged;
        _lastStatus = serverManager.IsRunning;

        // Poll every 2 seconds
        _timer = new System.Threading.Timer(CheckStatus, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
    }

    private void CheckStatus(object? state)
    {
        bool currentStatus = _serverManager.IsRunning;
        if (currentStatus != _lastStatus)
        {
            _lastStatus = currentStatus;
            _onStatusChanged(currentStatus);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
