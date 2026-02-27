using System.CommandLine;
using Geisterhand.Cli.Commands;

var rootCommand = new RootCommand("Geisterhand — Screen automation tool for Windows");

rootCommand.Add(StatusCommand.Create());
rootCommand.Add(ScreenshotCommand.Create());
rootCommand.Add(ClickCommand.Create());
rootCommand.Add(TypeCommand.Create());
rootCommand.Add(KeyCommand.Create());
rootCommand.Add(ScrollCommand.Create());
rootCommand.Add(ServerCommand.Create());
rootCommand.Add(RunCommand.Create());

var config = new CommandLineConfiguration(rootCommand);
return await config.InvokeAsync(args);
