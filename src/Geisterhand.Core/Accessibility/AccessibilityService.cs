using System.Diagnostics;
using System.Windows.Automation;
using Geisterhand.Core.Models;

namespace Geisterhand.Core.Accessibility;

public class AccessibilityService
{
    /// <summary>
    /// Get the UI Automation element for a window identified by handle.
    /// </summary>
    public AutomationElement GetWindowElement(IntPtr hWnd)
    {
        return AutomationElement.FromHandle(hWnd);
    }

    /// <summary>
    /// Get the UI Automation element for a process's main window.
    /// </summary>
    public AutomationElement GetApplicationElement(int pid)
    {
        var proc = Process.GetProcessById(pid);
        if (proc.MainWindowHandle == IntPtr.Zero)
            throw new InvalidOperationException($"Process {pid} has no main window.");
        return AutomationElement.FromHandle(proc.MainWindowHandle);
    }

    /// <summary>
    /// Build the accessibility tree for a given root element.
    /// </summary>
    public AccessibilityElement BuildTree(AutomationElement root, int maxDepth = 10, string format = "full")
    {
        return BuildTreeRecursive(root, [], maxDepth, 0, format);
    }

    private AccessibilityElement BuildTreeRecursive(
        AutomationElement element, List<int> currentPath, int maxDepth, int currentDepth, string format)
    {
        var current = element.Current;
        string role = RoleMap.ToAxRole(current.ControlType);
        string? title = string.IsNullOrEmpty(current.Name) ? null : current.Name;

        string? value = null;
        try
        {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern))
            {
                value = ((ValuePattern)pattern).Current.Value;
                if (string.IsNullOrEmpty(value)) value = null;
            }
        }
        catch { }

        List<AccessibilityElement>? children = null;

        if (currentDepth < maxDepth)
        {
            var childElements = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            if (childElements.Count > 0)
            {
                children = new List<AccessibilityElement>();
                for (int i = 0; i < childElements.Count; i++)
                {
                    var childPath = new List<int>(currentPath) { i };
                    children.Add(BuildTreeRecursive(childElements[i], childPath, maxDepth, currentDepth + 1, format));
                }
            }
        }

        if (format == "compact")
        {
            return new AccessibilityElement(
                Role: role,
                Title: title,
                Value: value,
                Path: new List<int>(currentPath),
                Children: children
            );
        }

        // Full format
        var rect = current.BoundingRectangle;
        var actions = GetAvailableActions(element);

        return new AccessibilityElement(
            Role: role,
            Title: title,
            Value: value,
            Description: string.IsNullOrEmpty(current.HelpText) ? null : current.HelpText,
            Position: rect.IsEmpty ? null : new ElementPosition(rect.X, rect.Y),
            Size: rect.IsEmpty ? null : new ElementSize(rect.Width, rect.Height),
            Children: children,
            Path: new List<int>(currentPath),
            Actions: actions.Count > 0 ? actions : null,
            IsEnabled: current.IsEnabled,
            IsFocused: current.HasKeyboardFocus
        );
    }

    /// <summary>
    /// Search for elements matching criteria.
    /// </summary>
    public List<AccessibilityElement> Search(
        AutomationElement root,
        string? role = null,
        string? title = null,
        string? titleContains = null,
        string? value = null,
        int maxResults = 50,
        int maxDepth = 10)
    {
        var results = new List<AccessibilityElement>();
        SearchRecursive(root, [], role, title, titleContains, value, maxResults, maxDepth, 0, results);
        return results;
    }

    private void SearchRecursive(
        AutomationElement element,
        List<int> currentPath,
        string? role,
        string? title,
        string? titleContains,
        string? value,
        int maxResults,
        int maxDepth,
        int currentDepth,
        List<AccessibilityElement> results)
    {
        if (results.Count >= maxResults) return;
        if (currentDepth > maxDepth) return;

        var current = element.Current;
        string elementRole = RoleMap.ToAxRole(current.ControlType);
        string? elementTitle = string.IsNullOrEmpty(current.Name) ? null : current.Name;

        string? elementValue = null;
        try
        {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern))
            {
                elementValue = ((ValuePattern)pattern).Current.Value;
                if (string.IsNullOrEmpty(elementValue)) elementValue = null;
            }
        }
        catch { }

        bool matches = true;
        if (role != null && !string.Equals(elementRole, role, StringComparison.OrdinalIgnoreCase))
            matches = false;
        if (title != null && !string.Equals(elementTitle, title, StringComparison.OrdinalIgnoreCase))
            matches = false;
        if (titleContains != null && (elementTitle == null || !elementTitle.Contains(titleContains, StringComparison.OrdinalIgnoreCase)))
            matches = false;
        if (value != null && !string.Equals(elementValue, value, StringComparison.OrdinalIgnoreCase))
            matches = false;

        if (matches && currentDepth > 0) // skip root
        {
            var rect = current.BoundingRectangle;
            var actions = GetAvailableActions(element);

            results.Add(new AccessibilityElement(
                Role: elementRole,
                Title: elementTitle,
                Value: elementValue,
                Position: rect.IsEmpty ? null : new ElementPosition(rect.X, rect.Y),
                Size: rect.IsEmpty ? null : new ElementSize(rect.Width, rect.Height),
                Path: new List<int>(currentPath),
                Actions: actions.Count > 0 ? actions : null,
                IsEnabled: current.IsEnabled,
                IsFocused: current.HasKeyboardFocus
            ));
        }

        if (results.Count >= maxResults) return;

        var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
        for (int i = 0; i < children.Count && results.Count < maxResults; i++)
        {
            var childPath = new List<int>(currentPath) { i };
            SearchRecursive(children[i], childPath, role, title, titleContains, value, maxResults, maxDepth, currentDepth + 1, results);
        }
    }

    /// <summary>
    /// Navigate to an element using a path of child indices.
    /// </summary>
    public AutomationElement NavigateToPath(AutomationElement root, IReadOnlyList<int> path)
    {
        var current = root;
        foreach (int index in path)
        {
            var children = current.FindAll(TreeScope.Children, Condition.TrueCondition);
            if (index < 0 || index >= children.Count)
                throw new InvalidOperationException($"Path index {index} out of range (children count: {children.Count})");
            current = children[index];
        }
        return current;
    }

    /// <summary>
    /// Perform an action on an element.
    /// </summary>
    public (string role, string? title) PerformAction(AutomationElement element, string action, string? value = null)
    {
        var current = element.Current;
        string role = RoleMap.ToAxRole(current.ControlType);
        string? title = string.IsNullOrEmpty(current.Name) ? null : current.Name;

        switch (action.ToLowerInvariant())
        {
            case "press":
            case "click":
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokeObj))
                {
                    ((InvokePattern)invokeObj).Invoke();
                }
                else if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? toggleObj))
                {
                    ((TogglePattern)toggleObj).Toggle();
                }
                else if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectObj))
                {
                    ((SelectionItemPattern)selectObj).Select();
                }
                else
                {
                    throw new InvalidOperationException($"Element does not support press/click action.");
                }
                break;

            case "setvalue":
            case "set_value":
                if (value == null)
                    throw new ArgumentException("Value is required for setValue action.");
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valueObj))
                {
                    ((ValuePattern)valueObj).SetValue(value);
                }
                else
                {
                    throw new InvalidOperationException("Element does not support setValue.");
                }
                break;

            case "focus":
                element.SetFocus();
                break;

            case "expand":
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandObj))
                {
                    ((ExpandCollapsePattern)expandObj).Expand();
                }
                break;

            case "collapse":
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? collapseObj))
                {
                    ((ExpandCollapsePattern)collapseObj).Collapse();
                }
                break;

            case "scroll":
                if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out object? scrollObj))
                {
                    ((ScrollPattern)scrollObj).Scroll(ScrollAmount.NoAmount, ScrollAmount.SmallIncrement);
                }
                break;

            default:
                throw new ArgumentException($"Unknown action: {action}");
        }

        return (role, title);
    }

    /// <summary>
    /// Get available actions for an element.
    /// </summary>
    public List<string> GetAvailableActions(AutomationElement element)
    {
        var actions = new List<string>();

        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out _))
            actions.Add("press");
        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out _))
            actions.Add("press");
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out _))
            actions.Add("setValue");
        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out _))
        {
            actions.Add("expand");
            actions.Add("collapse");
        }
        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out _))
            actions.Add("select");
        if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out _))
            actions.Add("scroll");

        actions.Add("focus");

        // Deduplicate
        return actions.Distinct().ToList();
    }
}
