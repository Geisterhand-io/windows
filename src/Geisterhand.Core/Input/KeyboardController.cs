using System.Runtime.InteropServices;
using Geisterhand.Core.Native;

namespace Geisterhand.Core.Input;

public class KeyboardController
{
    private static readonly int s_inputSize = Marshal.SizeOf<User32.INPUT>();

    /// <summary>
    /// Type a string of text using SendInput with KEYEVENTF_UNICODE.
    /// Works globally (to whichever window has focus).
    /// </summary>
    public int TypeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var inputs = new List<User32.INPUT>();
        foreach (char c in text)
        {
            // Key down
            inputs.Add(new User32.INPUT
            {
                Type = User32.INPUT_KEYBOARD,
                U = new User32.InputUnion
                {
                    ki = new User32.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = User32.KEYEVENTF_UNICODE,
                    }
                }
            });
            // Key up
            inputs.Add(new User32.INPUT
            {
                Type = User32.INPUT_KEYBOARD,
                U = new User32.InputUnion
                {
                    ki = new User32.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = User32.KEYEVENTF_UNICODE | User32.KEYEVENTF_KEYUP,
                    }
                }
            });
        }

        var arr = inputs.ToArray();
        User32.SendInput((uint)arr.Length, arr, s_inputSize);
        return text.Length;
    }

    /// <summary>
    /// Type text targeted to a specific window handle using PostMessage WM_CHAR.
    /// </summary>
    public int TypeTextToWindow(IntPtr hWnd, string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        foreach (char c in text)
        {
            User32.PostMessageW(hWnd, User32.WM_CHAR, c, 0);
        }
        return text.Length;
    }

    /// <summary>
    /// Press a key with optional modifiers using SendInput (global).
    /// </summary>
    public void PressKey(string keyName, IReadOnlyList<string>? modifiers = null)
    {
        if (!KeyCodeMap.TryGetVirtualKey(keyName, out ushort vk))
            throw new ArgumentException($"Unknown key: {keyName}");

        var inputs = new List<User32.INPUT>();

        // Press modifiers
        var modVks = ResolveModifiers(modifiers);
        foreach (var modVk in modVks)
        {
            inputs.Add(MakeKeyInput(modVk, down: true));
        }

        // Press key
        inputs.Add(MakeKeyInput(vk, down: true));
        inputs.Add(MakeKeyInput(vk, down: false));

        // Release modifiers in reverse order
        for (int i = modVks.Count - 1; i >= 0; i--)
        {
            inputs.Add(MakeKeyInput(modVks[i], down: false));
        }

        var arr = inputs.ToArray();
        User32.SendInput((uint)arr.Length, arr, s_inputSize);
    }

    /// <summary>
    /// Press a key targeted to a specific window handle using PostMessage.
    /// </summary>
    public void PressKeyToWindow(IntPtr hWnd, string keyName, IReadOnlyList<string>? modifiers = null)
    {
        if (!KeyCodeMap.TryGetVirtualKey(keyName, out ushort vk))
            throw new ArgumentException($"Unknown key: {keyName}");

        var modVks = ResolveModifiers(modifiers);

        // Press modifiers
        foreach (var modVk in modVks)
        {
            User32.PostMessageW(hWnd, User32.WM_KEYDOWN, modVk, MakeLParam(modVk, false));
        }

        // Press and release key
        User32.PostMessageW(hWnd, User32.WM_KEYDOWN, vk, MakeLParam(vk, false));
        User32.PostMessageW(hWnd, User32.WM_KEYUP, vk, MakeLParam(vk, true));

        // Release modifiers
        for (int i = modVks.Count - 1; i >= 0; i--)
        {
            User32.PostMessageW(hWnd, User32.WM_KEYUP, modVks[i], MakeLParam(modVks[i], true));
        }
    }

    private static List<ushort> ResolveModifiers(IReadOnlyList<string>? modifiers)
    {
        var result = new List<ushort>();
        if (modifiers == null) return result;

        foreach (var mod in modifiers)
        {
            if (KeyCodeMap.TryGetModifierKey(mod, out ushort modVk) && modVk != 0)
            {
                result.Add(modVk);
            }
        }
        return result;
    }

    private static User32.INPUT MakeKeyInput(ushort vk, bool down)
    {
        uint flags = down ? User32.KEYEVENTF_KEYDOWN : User32.KEYEVENTF_KEYUP;
        if (KeyCodeMap.IsExtendedKey(vk))
            flags |= User32.KEYEVENTF_EXTENDEDKEY;

        return new User32.INPUT
        {
            Type = User32.INPUT_KEYBOARD,
            U = new User32.InputUnion
            {
                ki = new User32.KEYBDINPUT
                {
                    wVk = vk,
                    wScan = 0,
                    dwFlags = flags,
                }
            }
        };
    }

    private static nint MakeLParam(ushort vk, bool keyUp)
    {
        // Simple lParam encoding for PostMessage
        int scanCode = 0;
        int lParam = 1; // repeat count = 1
        lParam |= (scanCode & 0xFF) << 16;
        if (KeyCodeMap.IsExtendedKey(vk))
            lParam |= 1 << 24;
        if (keyUp)
        {
            lParam |= 1 << 30; // previous key state
            lParam |= 1 << 31; // transition state
        }
        return (nint)lParam;
    }
}
