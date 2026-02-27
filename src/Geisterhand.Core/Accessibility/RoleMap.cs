using System.Windows.Automation;

namespace Geisterhand.Core.Accessibility;

/// <summary>
/// Maps UIA ControlType IDs to AX-style role strings for wire compatibility with the macOS API.
/// </summary>
public static class RoleMap
{
    private static readonly Dictionary<int, string> s_controlTypeToAxRole = new()
    {
        [ControlType.Button.Id] = "AXButton",
        [ControlType.Calendar.Id] = "AXGroup",
        [ControlType.CheckBox.Id] = "AXCheckBox",
        [ControlType.ComboBox.Id] = "AXComboBox",
        [ControlType.Custom.Id] = "AXGroup",
        [ControlType.DataGrid.Id] = "AXTable",
        [ControlType.DataItem.Id] = "AXRow",
        [ControlType.Document.Id] = "AXTextArea",
        [ControlType.Edit.Id] = "AXTextField",
        [ControlType.Group.Id] = "AXGroup",
        [ControlType.Header.Id] = "AXGroup",
        [ControlType.HeaderItem.Id] = "AXStaticText",
        [ControlType.Hyperlink.Id] = "AXLink",
        [ControlType.Image.Id] = "AXImage",
        [ControlType.List.Id] = "AXList",
        [ControlType.ListItem.Id] = "AXStaticText",
        [ControlType.Menu.Id] = "AXMenu",
        [ControlType.MenuBar.Id] = "AXMenuBar",
        [ControlType.MenuItem.Id] = "AXMenuItem",
        [ControlType.Pane.Id] = "AXGroup",
        [ControlType.ProgressBar.Id] = "AXProgressIndicator",
        [ControlType.RadioButton.Id] = "AXRadioButton",
        [ControlType.ScrollBar.Id] = "AXScrollBar",
        [ControlType.Separator.Id] = "AXSplitter",
        [ControlType.Slider.Id] = "AXSlider",
        [ControlType.Spinner.Id] = "AXIncrementor",
        [ControlType.SplitButton.Id] = "AXPopUpButton",
        [ControlType.StatusBar.Id] = "AXStaticText",
        [ControlType.Tab.Id] = "AXTabGroup",
        [ControlType.TabItem.Id] = "AXRadioButton",
        [ControlType.Table.Id] = "AXTable",
        [ControlType.Text.Id] = "AXStaticText",
        [ControlType.Thumb.Id] = "AXHandle",
        [ControlType.TitleBar.Id] = "AXStaticText",
        [ControlType.ToolBar.Id] = "AXToolbar",
        [ControlType.ToolTip.Id] = "AXHelpTag",
        [ControlType.Tree.Id] = "AXOutline",
        [ControlType.TreeItem.Id] = "AXRow",
        [ControlType.Window.Id] = "AXWindow",
    };

    /// <summary>
    /// Convert a UIA ControlType to its AX-style role string.
    /// </summary>
    public static string ToAxRole(ControlType controlType)
    {
        return s_controlTypeToAxRole.TryGetValue(controlType.Id, out var role)
            ? role
            : "AXGroup";
    }

    /// <summary>
    /// Convert a UIA ControlType ID to its AX-style role string.
    /// </summary>
    public static string ToAxRole(int controlTypeId)
    {
        return s_controlTypeToAxRole.TryGetValue(controlTypeId, out var role)
            ? role
            : "AXGroup";
    }

    /// <summary>
    /// Try to find the UIA ControlType matching an AX role string.
    /// Returns all matching ControlType IDs since the mapping isn't 1:1.
    /// </summary>
    public static List<int> FromAxRole(string axRole)
    {
        var result = new List<int>();
        foreach (var (ctId, role) in s_controlTypeToAxRole)
        {
            if (string.Equals(role, axRole, StringComparison.OrdinalIgnoreCase))
                result.Add(ctId);
        }
        return result;
    }
}
