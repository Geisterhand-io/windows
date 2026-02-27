using Geisterhand.Core.Server;

namespace Geisterhand.Tray;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ServerManager _serverManager;
    private readonly StatusMonitor _statusMonitor;
    private readonly ToolStripMenuItem _statusMenuItem;
    private readonly ToolStripMenuItem _startStopMenuItem;

    public TrayApplicationContext()
    {
        _serverManager = new ServerManager();

        _statusMenuItem = new ToolStripMenuItem("Server: Stopped")
        {
            Enabled = false
        };

        _startStopMenuItem = new ToolStripMenuItem("Start Server", null, OnStartStop);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_statusMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_startStopMenuItem);
        contextMenu.Items.Add(new ToolStripMenuItem("Restart Server", null, OnRestart));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Quit", null, OnQuit));

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateIcon(Color.Gray),
            Text = "Geisterhand",
            Visible = true,
            ContextMenuStrip = contextMenu
        };

        _notifyIcon.DoubleClick += OnStartStop;

        _statusMonitor = new StatusMonitor(_serverManager, OnStatusChanged);

        // Auto-start the server
        _ = StartServerAsync();
    }

    private async Task StartServerAsync()
    {
        try
        {
            await _serverManager.StartAsync();
            UpdateUI(true);
        }
        catch (Exception ex)
        {
            _notifyIcon.ShowBalloonTip(3000, "Geisterhand", $"Failed to start server: {ex.Message}", ToolTipIcon.Error);
            UpdateUI(false);
        }
    }

    private void OnStatusChanged(bool isRunning)
    {
        if (_notifyIcon.ContextMenuStrip?.InvokeRequired == true)
        {
            _notifyIcon.ContextMenuStrip.Invoke(() => UpdateUI(isRunning));
        }
        else
        {
            UpdateUI(isRunning);
        }
    }

    private void UpdateUI(bool isRunning)
    {
        if (isRunning)
        {
            _notifyIcon.Icon = CreateIcon(Color.LimeGreen);
            _notifyIcon.Text = $"Geisterhand - Running on port {_serverManager.Port}";
            _statusMenuItem.Text = $"Server: Running (port {_serverManager.Port})";
            _startStopMenuItem.Text = "Stop Server";
        }
        else
        {
            _notifyIcon.Icon = CreateIcon(Color.Gray);
            _notifyIcon.Text = "Geisterhand - Stopped";
            _statusMenuItem.Text = "Server: Stopped";
            _startStopMenuItem.Text = "Start Server";
        }
    }

    private async void OnStartStop(object? sender, EventArgs e)
    {
        if (_serverManager.IsRunning)
        {
            await _serverManager.StopAsync();
            UpdateUI(false);
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async void OnRestart(object? sender, EventArgs e)
    {
        await _serverManager.StopAsync();
        UpdateUI(false);
        await StartServerAsync();
    }

    private async void OnQuit(object? sender, EventArgs e)
    {
        _statusMonitor.Dispose();
        await _serverManager.StopAsync();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Application.Exit();
    }

    private static Icon CreateIcon(Color color)
    {
        var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1, 1, 14, 14);

            // Draw a small hand/ghost shape outline
            using var pen = new Pen(Color.White, 1.5f);
            g.DrawEllipse(pen, 3, 2, 10, 10);
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusMonitor.Dispose();
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
