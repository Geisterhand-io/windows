using System.Windows.Automation;
using Geisterhand.Core.Models;

namespace Geisterhand.Core.Accessibility;

public class MenuService
{
    /// <summary>
    /// Discover the menu bar and all menu items for a given window element.
    /// </summary>
    public List<MenuItem> GetMenuItems(AutomationElement windowElement)
    {
        var menus = new List<MenuItem>();

        // Find the menu bar
        var menuBar = windowElement.FindFirst(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuBar));

        if (menuBar == null)
            return menus;

        // Get top-level menu items
        var topLevelItems = menuBar.FindAll(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem));

        for (int i = 0; i < topLevelItems.Count; i++)
        {
            menus.Add(BuildMenuTree(topLevelItems[i]));
        }

        return menus;
    }

    private MenuItem BuildMenuTree(AutomationElement menuElement)
    {
        var current = menuElement.Current;
        string title = current.Name ?? "";
        bool enabled = current.IsEnabled;
        string? shortcut = GetShortcut(menuElement);

        List<MenuItem>? children = null;

        // Try to expand and get children
        bool wasExpanded = false;
        try
        {
            if (menuElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandObj))
            {
                var pattern = (ExpandCollapsePattern)expandObj;
                if (pattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                {
                    pattern.Expand();
                    wasExpanded = true;
                    Thread.Sleep(50); // brief pause for menu to render
                }
            }
        }
        catch { }

        var childItems = menuElement.FindAll(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem));

        if (childItems.Count > 0)
        {
            children = new List<MenuItem>();
            for (int i = 0; i < childItems.Count; i++)
            {
                children.Add(BuildMenuTree(childItems[i]));
            }
        }

        // Collapse back if we expanded
        if (wasExpanded)
        {
            try
            {
                if (menuElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? collapseObj))
                {
                    ((ExpandCollapsePattern)collapseObj).Collapse();
                }
            }
            catch { }
        }

        return new MenuItem(
            Title: title,
            Enabled: enabled,
            Shortcut: shortcut,
            Children: children
        );
    }

    /// <summary>
    /// Trigger a menu item by path (e.g., ["File", "New"]).
    /// </summary>
    public void TriggerMenuItem(AutomationElement windowElement, IReadOnlyList<string> path)
    {
        if (path.Count == 0)
            throw new ArgumentException("Menu path cannot be empty.");

        var menuBar = windowElement.FindFirst(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuBar));

        if (menuBar == null)
            throw new InvalidOperationException("No menu bar found.");

        AutomationElement current = menuBar;

        for (int i = 0; i < path.Count; i++)
        {
            var menuItem = FindMenuItemByTitle(current, path[i]);
            if (menuItem == null)
                throw new InvalidOperationException($"Menu item '{path[i]}' not found at level {i}.");

            if (i < path.Count - 1)
            {
                // Expand intermediate menu
                if (menuItem.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandObj))
                {
                    ((ExpandCollapsePattern)expandObj).Expand();
                    Thread.Sleep(100);
                }
                current = menuItem;
            }
            else
            {
                // Invoke the final item
                if (menuItem.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokeObj))
                {
                    ((InvokePattern)invokeObj).Invoke();
                }
                else if (menuItem.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandObj))
                {
                    // Some menu items use expand instead of invoke
                    ((ExpandCollapsePattern)expandObj).Expand();
                }
                else
                {
                    throw new InvalidOperationException($"Menu item '{path[i]}' cannot be triggered.");
                }
            }
        }
    }

    private AutomationElement? FindMenuItemByTitle(AutomationElement parent, string title)
    {
        var items = parent.FindAll(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem));

        for (int i = 0; i < items.Count; i++)
        {
            if (string.Equals(items[i].Current.Name, title, StringComparison.OrdinalIgnoreCase))
                return items[i];
        }

        // Try partial match
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Current.Name?.Contains(title, StringComparison.OrdinalIgnoreCase) == true)
                return items[i];
        }

        return null;
    }

    private static string? GetShortcut(AutomationElement menuElement)
    {
        try
        {
            var accessKey = menuElement.Current.AccessKey;
            if (!string.IsNullOrEmpty(accessKey))
                return accessKey;

            var acceleratorKey = menuElement.Current.AcceleratorKey;
            if (!string.IsNullOrEmpty(acceleratorKey))
                return acceleratorKey;
        }
        catch { }

        return null;
    }
}
