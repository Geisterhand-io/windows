using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Geisterhand.Core.Models;
using Geisterhand.Core.Screen;
using Geisterhand.Core.Server;

namespace Geisterhand.Cli.Commands;

public static class RunCommand
{
    public static Command Create()
    {
        var appArg = new Argument<string>("app") { Description = "Application to launch (process name, window title, or executable path)" };
        var portOption = new Option<int?>("--port") { Description = "Port for the scoped server (auto-assigned if not specified)" };

        var command = new Command("run", "Launch an app and start a scoped server for it");
        command.Add(appArg);
        command.Add(portOption);

        command.SetAction(async (parseResult, ct) =>
        {
            string app = parseResult.GetValue(appArg)!;
            int? port = parseResult.GetValue(portOption);

            var capture = new ScreenCaptureService();

            // Step 1: Try to find an already-running process
            var (process, appName) = FindExistingProcess(app, capture);

            // Step 2: If not found, launch it
            if (process == null)
            {
                (process, appName) = await LaunchAndFindProcess(app, capture, ct);
            }

            if (process == null || appName == null)
            {
                Console.Error.WriteLine($"Could not find or launch: {app}");
                return;
            }

            int serverPort = port ?? ServerManager.FindAvailablePort();
            var server = new GeisterhandServer(serverPort, targetPid: process.Id, targetAppName: appName);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            await server.StartAsync(cts.Token);

            var runResponse = new RunResponse(
                AppName: appName,
                Pid: process.Id,
                Port: serverPort,
                BaseUrl: $"http://127.0.0.1:{serverPort}"
            );
            Console.WriteLine(JsonSerializer.Serialize(runResponse, GeisterhandServer.JsonOptions));

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                await server.StopAsync();
            }
        });

        return command;
    }

    /// <summary>
    /// Try to find an already-running process by name or window title.
    /// </summary>
    private static (Process? process, string? appName) FindExistingProcess(string app, ScreenCaptureService capture)
    {
        // Try by process name
        var processes = Process.GetProcessesByName(app);
        if (processes.Length == 0)
            processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(app));

        foreach (var proc in processes)
        {
            try
            {
                if (proc.HasExited) continue;
                return (proc, proc.ProcessName);
            }
            catch { }
        }

        // Try by window title
        var hwnd = capture.FindWindowByAppName(app);
        if (hwnd != IntPtr.Zero)
        {
            int pid = capture.GetWindowProcessId(hwnd);
            try
            {
                var proc = Process.GetProcessById(pid);
                if (!proc.HasExited)
                    return (proc, proc.ProcessName);
            }
            catch { }
        }

        return (null, null);
    }

    /// <summary>
    /// Launch an app and find the resulting process.
    /// Handles modern Store/UWP apps where the initial broker process exits immediately
    /// and the real app spawns as a separate process.
    /// </summary>
    private static async Task<(Process? process, string? appName)> LaunchAndFindProcess(
        string app, ScreenCaptureService capture, CancellationToken ct)
    {
        // Snapshot PIDs of windows with matching names before launch
        var preExistingPids = new HashSet<int>();
        var existingWindows = capture.GetVisibleWindows();
        foreach (var w in existingWindows)
            preExistingPids.Add(w.ProcessId);

        // Try to launch
        Process? launchedProcess = null;
        try
        {
            launchedProcess = Process.Start(new ProcessStartInfo
            {
                FileName = app,
                UseShellExecute = true
            });
        }
        catch
        {
            return (null, null);
        }

        // Wait for a new window to appear
        // The launched broker process may have already exited (Store/UWP apps),
        // so we search for any new window matching the app name
        for (int attempt = 0; attempt < 60; attempt++)
        {
            await Task.Delay(100, ct);

            // First: check if the launched process itself has a window
            if (launchedProcess != null)
            {
                try
                {
                    launchedProcess.Refresh();
                    if (!launchedProcess.HasExited && launchedProcess.MainWindowHandle != IntPtr.Zero)
                        return (launchedProcess, launchedProcess.ProcessName);
                }
                catch { }
            }

            // Second: look for any new window whose process wasn't running before
            var currentWindows = capture.GetVisibleWindows();
            foreach (var w in currentWindows)
            {
                if (preExistingPids.Contains(w.ProcessId)) continue;

                // New window appeared — check if it matches our app name
                bool matches = w.ProcessName.Contains(app, StringComparison.OrdinalIgnoreCase)
                    || w.Title.Contains(app, StringComparison.OrdinalIgnoreCase);

                if (matches)
                {
                    try
                    {
                        var proc = Process.GetProcessById(w.ProcessId);
                        if (!proc.HasExited)
                            return (proc, proc.ProcessName);
                    }
                    catch { }
                }
            }

            // Third: on later attempts, accept any new visible window
            // (for apps like "calc" that open as "Calculator")
            if (attempt > 15)
            {
                foreach (var w in currentWindows)
                {
                    if (preExistingPids.Contains(w.ProcessId)) continue;
                    try
                    {
                        var proc = Process.GetProcessById(w.ProcessId);
                        if (!proc.HasExited)
                            return (proc, proc.ProcessName);
                    }
                    catch { }
                }
            }
        }

        return (null, null);
    }
}
