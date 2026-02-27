using Geisterhand.Core.Input;
using Xunit;

namespace Geisterhand.Tests.Input;

public class KeyCodeMapTests
{
    [Theory]
    [InlineData("a", 0x41)]
    [InlineData("z", 0x5A)]
    [InlineData("A", 0x41)]  // case-insensitive
    [InlineData("0", 0x30)]
    [InlineData("9", 0x39)]
    public void TryGetVirtualKey_Letters_And_Numbers(string keyName, int expectedVk)
    {
        Assert.True(KeyCodeMap.TryGetVirtualKey(keyName, out var vk));
        Assert.Equal((ushort)expectedVk, vk);
    }

    [Theory]
    [InlineData("return", KeyCodeMap.VK_RETURN)]
    [InlineData("enter", KeyCodeMap.VK_RETURN)]
    [InlineData("tab", KeyCodeMap.VK_TAB)]
    [InlineData("space", KeyCodeMap.VK_SPACE)]
    [InlineData("escape", KeyCodeMap.VK_ESCAPE)]
    [InlineData("esc", KeyCodeMap.VK_ESCAPE)]
    [InlineData("delete", KeyCodeMap.VK_BACK)]       // macOS delete = backspace
    [InlineData("forwarddelete", KeyCodeMap.VK_DELETE)]
    public void TryGetVirtualKey_SpecialKeys(string keyName, ushort expectedVk)
    {
        Assert.True(KeyCodeMap.TryGetVirtualKey(keyName, out var vk));
        Assert.Equal(expectedVk, vk);
    }

    [Theory]
    [InlineData("f1", KeyCodeMap.VK_F1)]
    [InlineData("f12", KeyCodeMap.VK_F12)]
    [InlineData("f20", KeyCodeMap.VK_F20)]
    public void TryGetVirtualKey_FunctionKeys(string keyName, ushort expectedVk)
    {
        Assert.True(KeyCodeMap.TryGetVirtualKey(keyName, out var vk));
        Assert.Equal(expectedVk, vk);
    }

    [Theory]
    [InlineData("uparrow", KeyCodeMap.VK_UP)]
    [InlineData("up", KeyCodeMap.VK_UP)]
    [InlineData("downarrow", KeyCodeMap.VK_DOWN)]
    [InlineData("leftarrow", KeyCodeMap.VK_LEFT)]
    [InlineData("rightarrow", KeyCodeMap.VK_RIGHT)]
    [InlineData("home", KeyCodeMap.VK_HOME)]
    [InlineData("end", KeyCodeMap.VK_END)]
    [InlineData("pageup", KeyCodeMap.VK_PRIOR)]
    [InlineData("pagedown", KeyCodeMap.VK_NEXT)]
    public void TryGetVirtualKey_NavigationKeys(string keyName, ushort expectedVk)
    {
        Assert.True(KeyCodeMap.TryGetVirtualKey(keyName, out var vk));
        Assert.Equal(expectedVk, vk);
    }

    [Theory]
    [InlineData("cmd", KeyCodeMap.VK_LWIN)]
    [InlineData("command", KeyCodeMap.VK_LWIN)]
    [InlineData("ctrl", KeyCodeMap.VK_CONTROL)]
    [InlineData("control", KeyCodeMap.VK_CONTROL)]
    [InlineData("alt", KeyCodeMap.VK_MENU)]
    [InlineData("option", KeyCodeMap.VK_MENU)]
    [InlineData("shift", KeyCodeMap.VK_SHIFT)]
    public void TryGetModifierKey_Maps_Correctly(string modifier, ushort expectedVk)
    {
        Assert.True(KeyCodeMap.TryGetModifierKey(modifier, out var vk));
        Assert.Equal(expectedVk, vk);
    }

    [Fact]
    public void TryGetVirtualKey_Unknown_ReturnsFalse()
    {
        Assert.False(KeyCodeMap.TryGetVirtualKey("nonexistent", out _));
    }

    [Theory]
    [InlineData(KeyCodeMap.VK_LEFT, true)]
    [InlineData(KeyCodeMap.VK_UP, true)]
    [InlineData(KeyCodeMap.VK_DELETE, true)]
    [InlineData(KeyCodeMap.VK_HOME, true)]
    [InlineData(KeyCodeMap.VK_LWIN, true)]
    [InlineData(KeyCodeMap.VK_RETURN, false)]
    [InlineData(KeyCodeMap.VK_SPACE, false)]
    [InlineData(0x41, false)]  // 'A' is not extended
    public void IsExtendedKey_ReturnsCorrectly(ushort vk, bool expected)
    {
        Assert.Equal(expected, KeyCodeMap.IsExtendedKey(vk));
    }

    [Theory]
    [InlineData('!', 0x31)]  // Shift+1
    [InlineData('@', 0x32)]  // Shift+2
    [InlineData('~', KeyCodeMap.VK_OEM_3)]
    [InlineData('{', KeyCodeMap.VK_OEM_4)]
    public void TryGetShiftedChar_Maps_Correctly(char c, ushort expectedBaseVk)
    {
        Assert.True(KeyCodeMap.TryGetShiftedChar(c, out var baseVk));
        Assert.Equal(expectedBaseVk, baseVk);
    }

    [Fact]
    public void TryGetShiftedChar_NonShifted_ReturnsFalse()
    {
        Assert.False(KeyCodeMap.TryGetShiftedChar('a', out _));
    }

    [Fact]
    public void AllKeyNames_ContainsExpectedEntries()
    {
        var keys = KeyCodeMap.AllKeyNames;
        Assert.Contains("return", keys);
        Assert.Contains("space", keys);
        Assert.Contains("a", keys);
        Assert.Contains("f1", keys);
    }
}
