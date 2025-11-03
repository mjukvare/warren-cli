using System.ComponentModel;
using Mjukvare.Warren.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands.Config;

public class InitCommand(SettingsManager settingsManager) : Command<InitCommand.InitSettings>
{
    public class InitSettings : CommandSettings
    {
        [CommandOption("-c|--config-name")]
        public string ConfigName { get; set; } = "default";

        [CommandOption("-h|--host")]
        [DefaultValue("localhost")]
        public string HostName { get; set; } = "localhost";
        
        [CommandOption("-p|--port")]
        [DefaultValue(5672)]
        public int Port { get; set; } = 5672;
        
        [CommandOption("-U|--username")]
        [DefaultValue("guest")]
        public string Username { get; set; } = "guest";
        
        [CommandOption("-P|--password")]
        [DefaultValue("guest")]
        public string Password { get; set; } = "guest";

        [CommandOption("-v|--virtual-host")]
        [DefaultValue("/")]
        public string VirtualHost { get; set; } = "/";
    }

    public override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        if (!settingsManager.ConfigDirectoryExists())
        {
            AnsiConsole.MarkupLine("Config directory not found.");
            AnsiConsole.MarkupLine($"Creating config directory at [green]{settingsManager.GetConfigDirectory()}[/].");
            settingsManager.CreateConfigDirectory();
        }
        
        if (settingsManager.RabbitMqConfigExists(settings.ConfigName))
        {
            AnsiConsole.MarkupLine($"[yellow]Config file already exists: {settings.ConfigName}[/]. Please provide a different config name.");
            return 0;
        }
        
        var rabbitMqSettings = new RabbitMqConfigData
        {
            HostName = settings.HostName,
            Port = settings.Port,
            Password = settings.Password,
            Username = settings.Username,
            VirtualHost = settings.VirtualHost
        };

        try
        {
            string configPath = settingsManager.SaveRabbitMqConfig(rabbitMqSettings, settings.ConfigName);
            settingsManager.UpdateDefaultRabbitMqConfig(configPath);
            AnsiConsole.MarkupLine($"[green]Config {settings.ConfigName} saved at: {configPath}[/]");
        }
        catch
        {
            AnsiConsole.MarkupLine("[red]Error: Could not save config file.[/]");
            return 1;
        }
        
        return 0;
    }
}

