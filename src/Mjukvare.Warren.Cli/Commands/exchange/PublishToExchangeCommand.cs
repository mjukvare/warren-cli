using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Mjukvare.Warren.Cli.Services;
using RabbitMQ.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands.exchange;

public class PublishToExchangeCommand(SettingsManager manager) : AsyncCommand<PublishToExchangeCommand.PublishToExchangeSettings>
{
    public class PublishToExchangeSettings : CommandSettings
    {
        [CommandArgument(0, "<exchange-name>")]
        [Description("The name of the exchange to publish to.")]
        public string ExchangeName { get; set; } = string.Empty;
        
        [CommandOption("-m|--message-json-file", isRequired: true)]
        [Description("The path to the json file containing the message data. Can be absolute or relative to the current working directory.")]
        public string MessageJsonFile { get; set; } = string.Empty;
        
        [CommandOption("--headers-json-file")]
        [Description("The path to the json file containing the headers data. Can be absolute or relative to the current working directory.")]
        public string HeadersJsonFile { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, PublishToExchangeSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(manager.GetDefaultRabbitMqConfigName()))
        {
            AnsiConsole.MarkupLine("[red]Error: No default config found.[/]");
            AnsiConsole.MarkupLine("Try [green]'warren config list'[/] to see available configs.");
            return 1;
        }
        
        AnsiConsole.MarkupLine($"[grey]Publishing to RabbitMQ using[/] [green]{manager.GetDefaultRabbitMqConfigName()}[/] [grey]config.[/]");
        
        string messageFilePath = Path.IsPathRooted(settings.MessageJsonFile) 
            ? settings.MessageJsonFile 
            : Path.Combine(Directory.GetCurrentDirectory(), settings.MessageJsonFile);

        if (!File.Exists(messageFilePath))
        {
            AnsiConsole.MarkupLine($"[red]Error: message data file at path: {messageFilePath}, cannot be found.[/]");
            return 1;
        }
        
        string messageContent;
        try
        {
            messageContent = await File.ReadAllTextAsync(messageFilePath, cancellationToken);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error reading file:[/] {ex.Message}");
            return 1;
        }
        
        Dictionary<string, object?> headers = new();
        if (!string.IsNullOrWhiteSpace(settings.HeadersJsonFile))
        {
            string headersFilePath = Path.IsPathRooted(settings.HeadersJsonFile) 
                ? settings.HeadersJsonFile 
                : Path.Combine(Directory.GetCurrentDirectory(), settings.HeadersJsonFile);
            
            if (!File.Exists(headersFilePath))
            {
                AnsiConsole.MarkupLine($"[red]Error: headers file at path: {headersFilePath}, cannot be found.[/]");
                return 1;
            }

            try
            {
                string headersJson = await File.ReadAllTextAsync(headersFilePath, cancellationToken);
                var headersDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(headersJson);

                if (headersDictionary is not null)
                {
                    foreach (KeyValuePair<string, JsonElement> kvp in headersDictionary)
                    {
                        headers[kvp.Key] = kvp.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error reading headers file:[/] {ex.Message}");
                return 1;
            }
        }

        return await AnsiConsole.Status()
            .StartAsync("Publishing message", async statusContext =>
            {
                statusContext.Spinner(Spinner.Known.Dots);
                statusContext.SpinnerStyle(Style.Parse("green"));

                RabbitMqConfigData rabbitMqConfig = manager.GetDefaultRabbitMqConfig();
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = rabbitMqConfig.HostName,
                        Port = rabbitMqConfig.Port,
                        Password = rabbitMqConfig.Password,
                        UserName = rabbitMqConfig.Username,
                        VirtualHost = rabbitMqConfig.VirtualHost
                    };

                    await using IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
                    await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                    byte[] body = Encoding.UTF8.GetBytes(messageContent);
                    var props = new BasicProperties();
                    props.Headers = headers;
                    
                    await channel.BasicPublishAsync(
                        settings.ExchangeName,
                        string.Empty,
                        true,
                        props,
                        body, 
                        cancellationToken
                    );
                    
                    AnsiConsole.MarkupLine($"[green]✓[/] Message published to exchange '{settings.ExchangeName}'");
                    
                    Table table = new Table()
                        .Border(TableBorder.Rounded)
                        .AddColumn("Property")
                        .AddColumn("Value");
                    
                    table.AddRow("Exchange", settings.ExchangeName);
                    table.AddRow("Message Size", $"{body.Length} bytes");
                    table.AddRow("File", Path.GetFileName(messageFilePath));
                    
                    AnsiConsole.Write(table);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]x Error publishing message:[/] {ex.Message}");
                    return 1;   
                }
                
                return 0;
            });
    }
}