using Mjukvare.Warren.Cli.Services;
using RabbitMQ.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands.Config;

public class TestConnectionCommand(SettingsManager manager) : AsyncCommand
{
    public override Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        return AnsiConsole.Status()
            .Start("Testing connection...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));

                try
                {
                    string rabbitMqConfigName = manager.GetDefaultRabbitMqConfigName();
                    AnsiConsole.MarkupLine($"[grey]Connecting to RabbitMQ using[/] [green]{rabbitMqConfigName}[/] [grey]config.[/]");
                    RabbitMqConfigData rabbitMqConfig = manager.GetDefaultRabbitMqConfig();
                    var factory = new ConnectionFactory
                    {
                        HostName = rabbitMqConfig.HostName,
                        UserName = rabbitMqConfig.Username,
                        Password = rabbitMqConfig.Password,
                        Port = rabbitMqConfig.Port,
                        VirtualHost = rabbitMqConfig.VirtualHost,
                        RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                    };

                    await using IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
                    await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    ctx.Status("Connected! Gathering server info...");

                    IDictionary<string, object?>? serverProperties = connection.ServerProperties;
                    AnsiConsole.MarkupLine("\n[green]✓[/] Connection successful!\n");

                    Table table = new Table()
                        .Border(TableBorder.Rounded)
                        .AddColumn("Property")
                        .AddColumn("Value");

                    table.AddRow("Host", factory.HostName);
                    table.AddRow("Port", factory.Port.ToString());
                    table.AddRow("Virtual Host", factory.VirtualHost);
                    table.AddRow("Username", factory.UserName);

                    if (serverProperties?.TryGetValue("product", out object? property) ?? false)
                    {
                        table.AddRow("Server Product", System.Text.Encoding.UTF8.GetString((byte[])property));
                    }

                    if (serverProperties?.TryGetValue("version", out object? serverProperty) ?? false)
                    {
                        table.AddRow("Server Version", System.Text.Encoding.UTF8.GetString((byte[])serverProperty));
                    }

                    AnsiConsole.Write(table);

                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[red]✗[/] Connection failed!");
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[yellow]Troubleshooting tips:[/]");
                    AnsiConsole.MarkupLine("  • Check if RabbitMQ is running");
                    AnsiConsole.MarkupLine("  • Verify the hostname and port are correct");
                    AnsiConsole.MarkupLine("  • Confirm username and password are valid");
                    AnsiConsole.MarkupLine("  • Check firewall settings");

                    return 1;
                }
            });
    }
}