using System.ComponentModel;
using Mjukvare.Warren.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands.Config;

public class SetDefaultCommand(SettingsManager manager) : Command<SetDefaultCommand.SetDefaultSettings>
{
    public class SetDefaultSettings : CommandSettings
    {
        [CommandArgument(0, "<config-name>")]
        [Description("The name of the config to set as default.")]
        public string ConfigName { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, SetDefaultSettings settings, CancellationToken cancellationToken)
    {
        if (!manager.RabbitMqConfigExists(settings.ConfigName))
        {
            AnsiConsole.MarkupLine($"[red]Error: Config file not found: {settings.ConfigName}[/]");
            AnsiConsole.MarkupLine("Try [green]'warren config list'[/] to see available configs.");
            return 1;
        }
        
        manager.UpdateDefaultRabbitMqConfig(settings.ConfigName);

        return 0;
    }
}