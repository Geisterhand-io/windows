namespace Geisterhand.Core.Input;

/// <summary>
/// Maps key name strings (matching macOS API contract) to Windows Virtual Key codes.
/// </summary>
public static class KeyCodeMap
{
    // Virtual Key constants
    public const ushort VK_BACK = 0x08;
    public const ushort VK_TAB = 0x09;
    public const ushort VK_CLEAR = 0x0C;
    public const ushort VK_RETURN = 0x0D;
    public const ushort VK_SHIFT = 0x10;
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_MENU = 0x12;      // Alt
    public const ushort VK_PAUSE = 0x13;
    public const ushort VK_CAPITAL = 0x14;    // Caps Lock
    public const ushort VK_ESCAPE = 0x1B;
    public const ushort VK_SPACE = 0x20;
    public const ushort VK_PRIOR = 0x21;      // Page Up
    public const ushort VK_NEXT = 0x22;       // Page Down
    public const ushort VK_END = 0x23;
    public const ushort VK_HOME = 0x24;
    public const ushort VK_LEFT = 0x25;
    public const ushort VK_UP = 0x26;
    public const ushort VK_RIGHT = 0x27;
    public const ushort VK_DOWN = 0x28;
    public const ushort VK_SELECT = 0x29;
    public const ushort VK_PRINT = 0x2A;
    public const ushort VK_SNAPSHOT = 0x2C;   // Print Screen
    public const ushort VK_INSERT = 0x2D;
    public const ushort VK_DELETE = 0x2E;
    public const ushort VK_LWIN = 0x5B;
    public const ushort VK_RWIN = 0x5C;
    public const ushort VK_APPS = 0x5D;
    public const ushort VK_NUMPAD0 = 0x60;
    public const ushort VK_NUMPAD1 = 0x61;
    public const ushort VK_NUMPAD2 = 0x62;
    public const ushort VK_NUMPAD3 = 0x63;
    public const ushort VK_NUMPAD4 = 0x64;
    public const ushort VK_NUMPAD5 = 0x65;
    public const ushort VK_NUMPAD6 = 0x66;
    public const ushort VK_NUMPAD7 = 0x67;
    public const ushort VK_NUMPAD8 = 0x68;
    public const ushort VK_NUMPAD9 = 0x69;
    public const ushort VK_MULTIPLY = 0x6A;
    public const ushort VK_ADD = 0x6B;
    public const ushort VK_SUBTRACT = 0x6D;
    public const ushort VK_DECIMAL = 0x6E;
    public const ushort VK_DIVIDE = 0x6F;
    public const ushort VK_F1 = 0x70;
    public const ushort VK_F2 = 0x71;
    public const ushort VK_F3 = 0x72;
    public const ushort VK_F4 = 0x73;
    public const ushort VK_F5 = 0x74;
    public const ushort VK_F6 = 0x75;
    public const ushort VK_F7 = 0x76;
    public const ushort VK_F8 = 0x77;
    public const ushort VK_F9 = 0x78;
    public const ushort VK_F10 = 0x79;
    public const ushort VK_F11 = 0x7A;
    public const ushort VK_F12 = 0x7B;
    public const ushort VK_F13 = 0x7C;
    public const ushort VK_F14 = 0x7D;
    public const ushort VK_F15 = 0x7E;
    public const ushort VK_F16 = 0x7F;
    public const ushort VK_F17 = 0x80;
    public const ushort VK_F18 = 0x81;
    public const ushort VK_F19 = 0x82;
    public const ushort VK_F20 = 0x83;
    public const ushort VK_NUMLOCK = 0x90;
    public const ushort VK_SCROLL = 0x91;
    public const ushort VK_LSHIFT = 0xA0;
    public const ushort VK_RSHIFT = 0xA1;
    public const ushort VK_LCONTROL = 0xA2;
    public const ushort VK_RCONTROL = 0xA3;
    public const ushort VK_LMENU = 0xA4;
    public const ushort VK_RMENU = 0xA5;
    public const ushort VK_OEM_1 = 0xBA;     // ;:
    public const ushort VK_OEM_PLUS = 0xBB;  // =+
    public const ushort VK_OEM_COMMA = 0xBC; // ,<
    public const ushort VK_OEM_MINUS = 0xBD; // -_
    public const ushort VK_OEM_PERIOD = 0xBE;// .>
    public const ushort VK_OEM_2 = 0xBF;     // /?
    public const ushort VK_OEM_3 = 0xC0;     // `~
    public const ushort VK_OEM_4 = 0xDB;     // [{
    public const ushort VK_OEM_5 = 0xDC;     // \|
    public const ushort VK_OEM_6 = 0xDD;     // ]}
    public const ushort VK_OEM_7 = 0xDE;     // '"

    private static readonly Dictionary<string, ushort> s_keyNameToVk = new(StringComparer.OrdinalIgnoreCase)
    {
        // Letters (VK codes for A-Z are 0x41-0x5A, same as ASCII uppercase)
        ["a"] = 0x41, ["b"] = 0x42, ["c"] = 0x43, ["d"] = 0x44,
        ["e"] = 0x45, ["f"] = 0x46, ["g"] = 0x47, ["h"] = 0x48,
        ["i"] = 0x49, ["j"] = 0x4A, ["k"] = 0x4B, ["l"] = 0x4C,
        ["m"] = 0x4D, ["n"] = 0x4E, ["o"] = 0x4F, ["p"] = 0x50,
        ["q"] = 0x51, ["r"] = 0x52, ["s"] = 0x53, ["t"] = 0x54,
        ["u"] = 0x55, ["v"] = 0x56, ["w"] = 0x57, ["x"] = 0x58,
        ["y"] = 0x59, ["z"] = 0x5A,

        // Numbers (VK codes for 0-9 are 0x30-0x39, same as ASCII)
        ["0"] = 0x30, ["1"] = 0x31, ["2"] = 0x32, ["3"] = 0x33,
        ["4"] = 0x34, ["5"] = 0x35, ["6"] = 0x36, ["7"] = 0x37,
        ["8"] = 0x38, ["9"] = 0x39,

        // Function keys
        ["f1"] = VK_F1, ["f2"] = VK_F2, ["f3"] = VK_F3, ["f4"] = VK_F4,
        ["f5"] = VK_F5, ["f6"] = VK_F6, ["f7"] = VK_F7, ["f8"] = VK_F8,
        ["f9"] = VK_F9, ["f10"] = VK_F10, ["f11"] = VK_F11, ["f12"] = VK_F12,
        ["f13"] = VK_F13, ["f14"] = VK_F14, ["f15"] = VK_F15, ["f16"] = VK_F16,
        ["f17"] = VK_F17, ["f18"] = VK_F18, ["f19"] = VK_F19, ["f20"] = VK_F20,

        // Special keys (matching macOS API key names)
        ["return"] = VK_RETURN, ["enter"] = VK_RETURN,
        ["tab"] = VK_TAB,
        ["space"] = VK_SPACE,
        ["delete"] = VK_BACK,          // macOS "delete" = backspace
        ["forwarddelete"] = VK_DELETE,  // macOS "forwardDelete"
        ["escape"] = VK_ESCAPE, ["esc"] = VK_ESCAPE,
        ["capslock"] = VK_CAPITAL,

        // Navigation
        ["uparrow"] = VK_UP, ["up"] = VK_UP,
        ["downarrow"] = VK_DOWN, ["down"] = VK_DOWN,
        ["leftarrow"] = VK_LEFT, ["left"] = VK_LEFT,
        ["rightarrow"] = VK_RIGHT, ["right"] = VK_RIGHT,
        ["home"] = VK_HOME,
        ["end"] = VK_END,
        ["pageup"] = VK_PRIOR,
        ["pagedown"] = VK_NEXT,

        // Modifiers (used when sending key combos)
        ["shift"] = VK_SHIFT,
        ["control"] = VK_CONTROL, ["ctrl"] = VK_CONTROL,
        ["alt"] = VK_MENU, ["option"] = VK_MENU,
        ["command"] = VK_LWIN, ["cmd"] = VK_LWIN, ["win"] = VK_LWIN,

        // Numpad
        ["numpad0"] = VK_NUMPAD0, ["numpad1"] = VK_NUMPAD1,
        ["numpad2"] = VK_NUMPAD2, ["numpad3"] = VK_NUMPAD3,
        ["numpad4"] = VK_NUMPAD4, ["numpad5"] = VK_NUMPAD5,
        ["numpad6"] = VK_NUMPAD6, ["numpad7"] = VK_NUMPAD7,
        ["numpad8"] = VK_NUMPAD8, ["numpad9"] = VK_NUMPAD9,

        // Symbols
        ["minus"] = VK_OEM_MINUS, ["-"] = VK_OEM_MINUS,
        ["equal"] = VK_OEM_PLUS, ["="] = VK_OEM_PLUS,
        ["leftbracket"] = VK_OEM_4, ["["] = VK_OEM_4,
        ["rightbracket"] = VK_OEM_6, ["]"] = VK_OEM_6,
        ["backslash"] = VK_OEM_5, ["\\"] = VK_OEM_5,
        ["semicolon"] = VK_OEM_1, [";"] = VK_OEM_1,
        ["quote"] = VK_OEM_7, ["'"] = VK_OEM_7,
        ["comma"] = VK_OEM_COMMA, [","] = VK_OEM_COMMA,
        ["period"] = VK_OEM_PERIOD, ["."] = VK_OEM_PERIOD,
        ["slash"] = VK_OEM_2, ["/"] = VK_OEM_2,
        ["grave"] = VK_OEM_3, ["`"] = VK_OEM_3,

        // Other
        ["printscreen"] = VK_SNAPSHOT,
        ["scrolllock"] = VK_SCROLL,
        ["numlock"] = VK_NUMLOCK,
        ["pause"] = VK_PAUSE,
        ["insert"] = VK_INSERT,
    };

    /// <summary>
    /// Extended keys that require KEYEVENTF_EXTENDEDKEY flag.
    /// </summary>
    private static readonly HashSet<ushort> s_extendedKeys =
    [
        VK_INSERT, VK_DELETE, VK_HOME, VK_END,
        VK_PRIOR, VK_NEXT,
        VK_LEFT, VK_UP, VK_RIGHT, VK_DOWN,
        VK_SNAPSHOT, VK_DIVIDE,
        VK_RCONTROL, VK_RMENU,
        VK_LWIN, VK_RWIN, VK_APPS,
        VK_NUMLOCK,
    ];

    /// <summary>
    /// Characters that require Shift to type on a US keyboard layout.
    /// Maps the shifted character to the VK code of the base key.
    /// </summary>
    private static readonly Dictionary<char, ushort> s_shiftCharToVk = new()
    {
        ['!'] = 0x31, ['@'] = 0x32, ['#'] = 0x33, ['$'] = 0x34,
        ['%'] = 0x35, ['^'] = 0x36, ['&'] = 0x37, ['*'] = 0x38,
        ['('] = 0x39, [')'] = 0x30,
        ['_'] = VK_OEM_MINUS, ['+'] = VK_OEM_PLUS,
        ['{'] = VK_OEM_4, ['}'] = VK_OEM_6,
        ['|'] = VK_OEM_5, [':'] = VK_OEM_1,
        ['"'] = VK_OEM_7, ['<'] = VK_OEM_COMMA,
        ['>'] = VK_OEM_PERIOD, ['?'] = VK_OEM_2,
        ['~'] = VK_OEM_3,
    };

    /// <summary>
    /// Maps a modifier name from the macOS API contract to a Windows VK code.
    /// </summary>
    private static readonly Dictionary<string, ushort> s_modifierMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cmd"] = VK_LWIN,
        ["command"] = VK_LWIN,
        ["ctrl"] = VK_CONTROL,
        ["control"] = VK_CONTROL,
        ["alt"] = VK_MENU,
        ["option"] = VK_MENU,
        ["shift"] = VK_SHIFT,
        ["fn"] = 0, // No direct Windows equivalent; ignore
    };

    /// <summary>
    /// Try to resolve a key name to a Windows Virtual Key code.
    /// </summary>
    public static bool TryGetVirtualKey(string keyName, out ushort vk)
    {
        return s_keyNameToVk.TryGetValue(keyName, out vk);
    }

    /// <summary>
    /// Try to resolve a modifier name to a Windows Virtual Key code.
    /// </summary>
    public static bool TryGetModifierKey(string modifier, out ushort vk)
    {
        return s_modifierMap.TryGetValue(modifier, out vk);
    }

    /// <summary>
    /// Returns true if the given VK code is an extended key (requires KEYEVENTF_EXTENDEDKEY).
    /// </summary>
    public static bool IsExtendedKey(ushort vk) => s_extendedKeys.Contains(vk);

    /// <summary>
    /// Try to resolve a character that requires Shift (e.g. '!' → VK_1 + Shift).
    /// </summary>
    public static bool TryGetShiftedChar(char c, out ushort baseVk)
    {
        return s_shiftCharToVk.TryGetValue(c, out baseVk);
    }

    /// <summary>
    /// Returns all known key names.
    /// </summary>
    public static IReadOnlyCollection<string> AllKeyNames => s_keyNameToVk.Keys;

    /// <summary>
    /// Returns all known modifier names.
    /// </summary>
    public static IReadOnlyCollection<string> AllModifierNames => s_modifierMap.Keys;
}
