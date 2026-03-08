using System.Diagnostics;
using System.Text.RegularExpressions;
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

        // Full format — includes extended state fields
        var rect = current.BoundingRectangle;
        var actions = GetAvailableActions(element);

        // Extended state fields
        string? automationId = string.IsNullOrEmpty(current.AutomationId) ? null : current.AutomationId;
        string? className = string.IsNullOrEmpty(current.ClassName) ? null : current.ClassName;
        string controlTypeName = current.ControlType.ProgrammaticName.Replace("ControlType.", "");

        bool? isExpanded = null;
        try
        {
            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? ecPattern))
            {
                var state = ((ExpandCollapsePattern)ecPattern).Current.ExpandCollapseState;
                isExpanded = state == ExpandCollapseState.Expanded || state == ExpandCollapseState.PartiallyExpanded;
            }
        }
        catch { }

        bool? isSelected = null;
        try
        {
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selPattern))
            {
                isSelected = ((SelectionItemPattern)selPattern).Current.IsSelected;
            }
        }
        catch { }

        bool? isChecked = null;
        try
        {
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togPattern))
            {
                var togState = ((TogglePattern)togPattern).Current.ToggleState;
                isChecked = togState == ToggleState.On;
            }
        }
        catch { }

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
            IsFocused: current.HasKeyboardFocus,
            AutomationId: automationId,
            ClassName: className,
            IsExpanded: isExpanded,
            IsSelected: isSelected,
            IsChecked: isChecked,
            IsOffscreen: current.IsOffscreen,
            ControlType: controlTypeName,
            ProcessId: current.ProcessId
        );
    }

    /// <summary>
    /// Search for elements matching criteria (with regex, automationId, state filters).
    /// </summary>
    public List<AccessibilityElement> Search(
        AutomationElement root,
        string? role = null,
        string? title = null,
        string? titleContains = null,
        string? value = null,
        int maxResults = 50,
        int maxDepth = 10,
        string? titleRegex = null,
        string? valueRegex = null,
        string? automationId = null,
        bool? enabledOnly = null,
        bool? visibleOnly = null)
    {
        Regex? titleRx = titleRegex != null ? new Regex(titleRegex, RegexOptions.IgnoreCase) : null;
        Regex? valueRx = valueRegex != null ? new Regex(valueRegex, RegexOptions.IgnoreCase) : null;

        var results = new List<AccessibilityElement>();
        SearchRecursive(root, [], role, title, titleContains, value, maxResults, maxDepth, 0, results,
            titleRx, valueRx, automationId, enabledOnly, visibleOnly);
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
        List<AccessibilityElement> results,
        Regex? titleRegex = null,
        Regex? valueRegex = null,
        string? automationId = null,
        bool? enabledOnly = null,
        bool? visibleOnly = null)
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
        if (titleRegex != null && (elementTitle == null || !titleRegex.IsMatch(elementTitle)))
            matches = false;
        if (valueRegex != null && (elementValue == null || !valueRegex.IsMatch(elementValue)))
            matches = false;
        if (automationId != null && !string.Equals(current.AutomationId, automationId, StringComparison.OrdinalIgnoreCase))
            matches = false;
        if (enabledOnly == true && !current.IsEnabled)
            matches = false;
        if (visibleOnly == true && current.IsOffscreen)
            matches = false;

        if (matches && currentDepth > 0) // skip root
        {
            var rect = current.BoundingRectangle;
            var actions = GetAvailableActions(element);

            string? aid = string.IsNullOrEmpty(current.AutomationId) ? null : current.AutomationId;
            string? cn = string.IsNullOrEmpty(current.ClassName) ? null : current.ClassName;

            results.Add(new AccessibilityElement(
                Role: elementRole,
                Title: elementTitle,
                Value: elementValue,
                Position: rect.IsEmpty ? null : new ElementPosition(rect.X, rect.Y),
                Size: rect.IsEmpty ? null : new ElementSize(rect.Width, rect.Height),
                Path: new List<int>(currentPath),
                Actions: actions.Count > 0 ? actions : null,
                IsEnabled: current.IsEnabled,
                IsFocused: current.HasKeyboardFocus,
                AutomationId: aid,
                ClassName: cn,
                IsOffscreen: current.IsOffscreen,
                ControlType: current.ControlType.ProgrammaticName.Replace("ControlType.", ""),
                ProcessId: current.ProcessId
            ));
        }

        if (results.Count >= maxResults) return;

        var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
        for (int i = 0; i < children.Count && results.Count < maxResults; i++)
        {
            var childPath = new List<int>(currentPath) { i };
            SearchRecursive(children[i], childPath, role, title, titleContains, value, maxResults, maxDepth, currentDepth + 1, results,
                titleRegex, valueRegex, automationId, enabledOnly, visibleOnly);
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
    /// Navigate relative to an element: parent, next_sibling, prev_sibling, first_child, last_child.
    /// </summary>
    public AutomationElement NavigateRelative(AutomationElement element, string direction)
    {
        var walker = TreeWalker.RawViewWalker;
        AutomationElement? result = direction.ToLowerInvariant() switch
        {
            "parent" => walker.GetParent(element),
            "next_sibling" => walker.GetNextSibling(element),
            "prev_sibling" => walker.GetPreviousSibling(element),
            "first_child" => walker.GetFirstChild(element),
            "last_child" => walker.GetLastChild(element),
            _ => throw new ArgumentException($"Unknown direction: {direction}")
        };

        if (result == null)
            throw new InvalidOperationException($"No element found in direction '{direction}'.");

        return result;
    }

    /// <summary>
    /// Wait for an element matching criteria to appear.
    /// </summary>
    public async Task<List<AccessibilityElement>> WaitForElement(
        AutomationElement root,
        string? role, string? title, string? titleContains, string? value,
        int timeoutMs, int pollIntervalMs, int maxDepth)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var results = Search(root, role, title, titleContains, value, maxResults: 1, maxDepth: maxDepth);
                if (results.Count > 0)
                    return results;
            }
            catch { }

            await Task.Delay(pollIntervalMs);
        }

        return [];
    }

    /// <summary>
    /// Wait for an element at a path to match a condition.
    /// </summary>
    public async Task<bool> WaitForCondition(
        AutomationElement root, IReadOnlyList<int> path,
        string condition, string? expectedValue,
        int timeoutMs, int pollIntervalMs)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                bool met = condition.ToLowerInvariant() switch
                {
                    "exists" => CheckExists(root, path),
                    "not_exists" => !CheckExists(root, path),
                    "enabled" => CheckEnabled(root, path, true),
                    "disabled" => CheckEnabled(root, path, false),
                    "expanded" => CheckExpandState(root, path, true),
                    "collapsed" => CheckExpandState(root, path, false),
                    "value_equals" => CheckValue(root, path, expectedValue, exact: true),
                    "value_contains" => CheckValue(root, path, expectedValue, exact: false),
                    _ => throw new ArgumentException($"Unknown condition: {condition}")
                };

                if (met) return true;
            }
            catch (ArgumentException) { throw; }
            catch { }

            await Task.Delay(pollIntervalMs);
        }

        return false;
    }

    private bool CheckExists(AutomationElement root, IReadOnlyList<int> path)
    {
        try { NavigateToPath(root, path); return true; }
        catch { return false; }
    }

    private bool CheckEnabled(AutomationElement root, IReadOnlyList<int> path, bool expected)
    {
        var el = NavigateToPath(root, path);
        return el.Current.IsEnabled == expected;
    }

    private bool CheckExpandState(AutomationElement root, IReadOnlyList<int> path, bool expanded)
    {
        var el = NavigateToPath(root, path);
        if (el.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? pat))
        {
            var state = ((ExpandCollapsePattern)pat).Current.ExpandCollapseState;
            return expanded
                ? (state == ExpandCollapseState.Expanded || state == ExpandCollapseState.PartiallyExpanded)
                : state == ExpandCollapseState.Collapsed;
        }
        return false;
    }

    private bool CheckValue(AutomationElement root, IReadOnlyList<int> path, string? expected, bool exact)
    {
        var el = NavigateToPath(root, path);
        if (el.TryGetCurrentPattern(ValuePattern.Pattern, out object? pat))
        {
            var val = ((ValuePattern)pat).Current.Value;
            if (exact) return string.Equals(val, expected, StringComparison.Ordinal);
            return val?.Contains(expected ?? "", StringComparison.OrdinalIgnoreCase) == true;
        }
        return false;
    }

    /// <summary>
    /// Get element at screen coordinates.
    /// </summary>
    public AccessibilityElement? GetElementAtPoint(int x, int y)
    {
        try
        {
            var element = AutomationElement.FromPoint(new System.Windows.Point(x, y));
            if (element == null) return null;

            var current = element.Current;
            string role = RoleMap.ToAxRole(current.ControlType);
            var rect = current.BoundingRectangle;
            var actions = GetAvailableActions(element);

            return new AccessibilityElement(
                Role: role,
                Title: string.IsNullOrEmpty(current.Name) ? null : current.Name,
                Value: null,
                Position: rect.IsEmpty ? null : new ElementPosition(rect.X, rect.Y),
                Size: rect.IsEmpty ? null : new ElementSize(rect.Width, rect.Height),
                Actions: actions.Count > 0 ? actions : null,
                IsEnabled: current.IsEnabled,
                IsFocused: current.HasKeyboardFocus,
                AutomationId: string.IsNullOrEmpty(current.AutomationId) ? null : current.AutomationId,
                ClassName: string.IsNullOrEmpty(current.ClassName) ? null : current.ClassName,
                ControlType: current.ControlType.ProgrammaticName.Replace("ControlType.", ""),
                ProcessId: current.ProcessId
            );
        }
        catch
        {
            return null;
        }
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
