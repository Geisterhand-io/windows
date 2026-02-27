using System.Text.Json;
using Geisterhand.Core.Models;
using Xunit;

namespace Geisterhand.Tests.Models;

public class ApiModelsTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void StatusResponse_Serializes_SnakeCase()
    {
        var response = new StatusResponse(
            Status: "ok",
            Version: "1.0.0",
            Platform: "windows",
            ApiVersion: "1",
            Permissions: new PermissionsInfo(Accessibility: true, ScreenRecording: true),
            RunningApplications: [new RunningApplication("Notepad", null, 1234, true)]
        );

        var json = JsonSerializer.Serialize(response, s_options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("ok", root.GetProperty("status").GetString());
        Assert.Equal("windows", root.GetProperty("platform").GetString());
        Assert.Equal("1.0.0", root.GetProperty("version").GetString());
        Assert.True(root.GetProperty("permissions").GetProperty("accessibility").GetBoolean());
        Assert.Equal(1234, root.GetProperty("running_applications")[0].GetProperty("pid").GetInt32());
        Assert.True(root.GetProperty("running_applications")[0].GetProperty("is_active").GetBoolean());
    }

    [Fact]
    public void ClickRequest_Deserializes_SnakeCase()
    {
        var json = """{"x":100,"y":200,"button":"right","click_type":"double","app_name":"notepad"}""";
        var request = JsonSerializer.Deserialize<ClickRequest>(json, s_options)!;

        Assert.Equal(100, request.X);
        Assert.Equal(200, request.Y);
        Assert.Equal("right", request.Button);
        Assert.Equal("double", request.ClickType);
        Assert.Equal("notepad", request.AppName);
    }

    [Fact]
    public void TypeRequest_Deserializes_Defaults()
    {
        var json = """{"text":"hello"}""";
        var request = JsonSerializer.Deserialize<TypeRequest>(json, s_options)!;

        Assert.Equal("hello", request.Text);
        Assert.False(request.UseClipboard);
        Assert.Null(request.AppName);
        Assert.Null(request.Pid);
    }

    [Fact]
    public void KeyRequest_Deserializes_WithModifiers()
    {
        var json = """{"key":"c","modifiers":["cmd","shift"],"pid":5678}""";
        var request = JsonSerializer.Deserialize<KeyRequest>(json, s_options)!;

        Assert.Equal("c", request.Key);
        Assert.NotNull(request.Modifiers);
        Assert.Equal(2, request.Modifiers.Count);
        Assert.Contains("cmd", request.Modifiers);
        Assert.Contains("shift", request.Modifiers);
        Assert.Equal(5678, request.Pid);
    }

    [Fact]
    public void ScrollRequest_Deserializes_WithDefaults()
    {
        var json = """{"x":500,"y":300}""";
        var request = JsonSerializer.Deserialize<ScrollRequest>(json, s_options)!;

        Assert.Equal(500, request.X);
        Assert.Equal(300, request.Y);
        Assert.Equal(0, request.DeltaX);
        Assert.Equal(0, request.DeltaY);
    }

    [Fact]
    public void ScreenshotResponse_Serializes_AllFields()
    {
        var response = new ScreenshotResponse(
            Image: "base64data",
            Format: "png",
            Width: 1920,
            Height: 1080
        );

        var json = JsonSerializer.Serialize(response, s_options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("base64data", root.GetProperty("image").GetString());
        Assert.Equal("png", root.GetProperty("format").GetString());
        Assert.Equal(1920, root.GetProperty("width").GetInt32());
        Assert.Equal(1080, root.GetProperty("height").GetInt32());
    }

    [Fact]
    public void AccessibilityElement_Serializes_WithNulls()
    {
        var element = new AccessibilityElement(
            Role: "AXButton",
            Title: "OK",
            Position: new ElementPosition(10, 20),
            Size: new ElementSize(100, 30)
        );

        var json = JsonSerializer.Serialize(element, s_options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("AXButton", root.GetProperty("role").GetString());
        Assert.Equal("OK", root.GetProperty("title").GetString());
        Assert.False(root.TryGetProperty("value", out _));  // null → omitted
        Assert.Equal(10, root.GetProperty("position").GetProperty("x").GetDouble());
    }

    [Fact]
    public void ErrorResponse_Serializes()
    {
        var error = new ErrorResponse(Error: "not_found", Detail: "No such process");
        var json = JsonSerializer.Serialize(error, s_options);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("not_found", doc.RootElement.GetProperty("error").GetString());
        Assert.Equal("No such process", doc.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public void MenuItem_Serializes_Nested()
    {
        var menu = new MenuItem(
            Title: "File",
            Children: [
                new MenuItem(Title: "New", Shortcut: "Ctrl+N"),
                new MenuItem(Title: "Open", Shortcut: "Ctrl+O"),
            ]
        );

        var json = JsonSerializer.Serialize(menu, s_options);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("File", root.GetProperty("title").GetString());
        Assert.Equal(2, root.GetProperty("children").GetArrayLength());
        Assert.Equal("Ctrl+N", root.GetProperty("children")[0].GetProperty("shortcut").GetString());
    }

    [Fact]
    public void WaitRequest_Deserializes()
    {
        var json = """{"seconds":2.5}""";
        var request = JsonSerializer.Deserialize<WaitRequest>(json, s_options)!;
        Assert.Equal(2.5, request.Seconds);
    }

    [Fact]
    public void RunResponse_Serializes()
    {
        var response = new RunResponse("Notepad", 1234, 7677, "http://127.0.0.1:7677");
        var json = JsonSerializer.Serialize(response, s_options);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("Notepad", doc.RootElement.GetProperty("app_name").GetString());
        Assert.Equal(7677, doc.RootElement.GetProperty("port").GetInt32());
        Assert.Equal("http://127.0.0.1:7677", doc.RootElement.GetProperty("base_url").GetString());
    }
}
