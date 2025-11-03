using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands;

public class WelcomeCommand : Command<WelcomeCommand.Settings>
{
    public class Settings : CommandSettings;

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(
            new FigletText("Warren")
                .LeftJustified()
                .Color(Color.Blue));
        
        AnsiConsole.MarkupLine("[grey]RabbitMQ CLI Utility[/]\n");
        
        AnsiConsole.MarkupLine("Available commands:");
        AnsiConsole.MarkupLine("  [blue]warren config[/]               - Manage configurations");
        AnsiConsole.MarkupLine("  [blue]warren config init[/]          - Initialize a new configuration");
        AnsiConsole.MarkupLine("  [blue]warren config test[/]          - Test if you can connect to RabbitMQ server");
        AnsiConsole.MarkupLine("  [blue]warren config list[/]          - List all configurations");
        AnsiConsole.MarkupLine("  [blue]warren config set-default[/]   - Set a configuration as default");
        AnsiConsole.MarkupLine("  [blue]warren exchange publish[/]     - Publish a message to an exchange");
        
        AnsiConsole.MarkupLine("Run [green]warren --help[/] for more information.");
        
        return 0;
    }
}