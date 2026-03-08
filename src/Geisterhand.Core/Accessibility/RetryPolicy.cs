using System.Windows.Automation;

namespace Geisterhand.Core.Accessibility;

public static class RetryPolicy
{
    /// <summary>
    /// Execute a UIA operation with retry on transient failures.
    /// </summary>
    public static T Execute<T>(Func<T> action, int maxRetries = 3, int baseDelayMs = 100)
    {
        Exception? lastException = null;
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return action();
            }
            catch (ElementNotAvailableException ex)
            {
                lastException = ex;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("element"))
            {
                lastException = ex;
            }

            if (i < maxRetries)
            {
                int delay = baseDelayMs * (1 << i); // exponential backoff
                Thread.Sleep(delay);
            }
        }

        throw lastException!;
    }

    /// <summary>
    /// Execute an async UIA operation with retry.
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, int maxRetries = 3, int baseDelayMs = 100)
    {
        Exception? lastException = null;
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await action();
            }
            catch (ElementNotAvailableException ex)
            {
                lastException = ex;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("element"))
            {
                lastException = ex;
            }

            if (i < maxRetries)
            {
                int delay = baseDelayMs * (1 << i);
                await Task.Delay(delay);
            }
        }

        throw lastException!;
    }
}
