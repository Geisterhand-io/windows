using System.CommandLine;
using Geisterhand.Cli.Commands;

var rootCommand = new RootCommand("Geisterhand — Screen automation tool for Windows");

rootCommand.Add(StatusCommand.Create());
rootCommand.Add(ScreenshotCommand.Create());
rootCommand.Add(ClickCommand.Create());
rootCommand.Add(ClickWindowCommand.Create());
rootCommand.Add(TypeCommand.Create());
rootCommand.Add(KeyCommand.Create());
rootCommand.Add(ScrollCommand.Create());
rootCommand.Add(ServerCommand.Create());
rootCommand.Add(RunCommand.Create());
rootCommand.Add(MouseMoveCommand.Create());
rootCommand.Add(DragCommand.Create());
rootCommand.Add(WindowCommand.Create());
rootCommand.Add(ClipboardCommand.Create());
rootCommand.Add(WaitForCommand.Create());
rootCommand.Add(HighlightCommand.Create());
rootCommand.Add(ScreenshotDiffCommand.Create());
rootCommand.Add(KeyStateCommand.Create());

var config = new CommandLineConfiguration(rootCommand);
return await config.InvokeAsync(args);
