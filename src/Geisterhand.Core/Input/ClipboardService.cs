namespace Geisterhand.Core.Input;

public class ClipboardService
{
    /// <summary>
    /// Get text from clipboard. Runs on STA thread since WPF Clipboard requires it.
    /// </summary>
    public string? GetText()
    {
        string? result = null;
        var thread = new Thread(() =>
        {
            try
            {
                if (System.Windows.Clipboard.ContainsText())
                    result = System.Windows.Clipboard.GetText();
            }
            catch { }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join(5000);
        return result;
    }

    /// <summary>
    /// Set text to clipboard. Runs on STA thread.
    /// </summary>
    public void SetText(string text)
    {
        var thread = new Thread(() =>
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch { }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join(5000);
    }
}
