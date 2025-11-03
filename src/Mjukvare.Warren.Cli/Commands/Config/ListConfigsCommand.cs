using Mjukvare.Warren.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands.Config;

public class ListConfigsCommand(SettingsManager manager) : Command<ListConfigsCommand.ListConfigsSettings>
{
    public class ListConfigsSettings : CommandSettings;

    public override int Execute(CommandContext context, ListConfigsSettings settings, CancellationToken cancellationToken)
    {
        foreach (FileInfo fileInfo in manager.ListRabbitMqConfigFiles().ToList())
        {
            AnsiConsole.MarkupLine($"[green]{fileInfo.Name.Replace(".json", "")}[/]");
        }

        return 0;
    }
}