using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mjukvare.Warren.Cli;
using Mjukvare.Warren.Cli.Commands;
using Mjukvare.Warren.Cli.Commands.Config;
using Mjukvare.Warren.Cli.Commands.exchange;
using Mjukvare.Warren.Cli.DependencyInjection;
using Mjukvare.Warren.Cli.Services;
using Spectre.Console.Cli;

string settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".warren");
var appSettings = new AppSettings
{
    ConfigDirectory = settingsFolder,
    DefaultConfigPath = string.Empty
};

if (File.Exists(appSettings.DefaultConfigFile))
{
    JsonSerializerOptions jsonSerializerOptions = AppJsonSettings.GetDefaultOptions();
    DefaultConfig defaultConfig = JsonSerializer.Deserialize<DefaultConfig>(
                                       File.ReadAllText(appSettings.DefaultConfigFile), 
                                       jsonSerializerOptions)
                                   ?? throw new Exception();
    appSettings.DefaultConfigPath = defaultConfig.DefaultConfigPath;
}

var services = new ServiceCollection();
var registrar = new TypeRegistrar(services);

services.AddSingleton<SettingsManager>();
services.AddSingleton(appSettings);

var app = new CommandApp(registrar);

app.SetDefaultCommand<WelcomeCommand>();
app.Configure(configurator =>
{
    configurator.SetApplicationName("warren");

    configurator.AddCommand<VersionCommand>("version");
    
    configurator.AddBranch("config", config =>
    {
        config.AddCommand<InitCommand>("init");
        config.AddCommand<TestConnectionCommand>("test");
        config.AddCommand<ListConfigsCommand>("list");
        config.AddCommand<SetDefaultCommand>("set-default");
    });

    configurator.AddBranch("exchange", exchangeConfig =>
    {
        exchangeConfig.AddCommand<PublishToExchangeCommand>("publish");
    });
});

app.Run(args);