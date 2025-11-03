using System.Text.Json;
using Mjukvare.Warren.Cli.Commands;
using Spectre.Console;
using Spectre.Console.Json;

namespace Mjukvare.Warren.Cli.Services;

public sealed class SettingsManager(AppSettings settings)
{
    public string GetConfigDirectory() => settings.ConfigDirectory;

    public bool ConfigDirectoryExists()
    {
        return Directory.Exists(settings.ConfigDirectory);
    }

    public bool CreateConfigDirectory()
    {
        if (ConfigDirectoryExists()) return false;
        Directory.CreateDirectory(settings.ConfigDirectory);
        
        return true;
    }

    public IEnumerable<FileInfo> ListRabbitMqConfigFiles()
    {
        List<FileInfo> files = Directory.EnumerateFiles(GetConfigDirectory())
            .Select(f => new FileInfo(f))
            .Where(f => f.Extension.Equals(".json"))
            .Where(f => !f.Name.Equals("config.json"))
            .ToList();

        return files;
    }
    
    public void UpdateDefaultRabbitMqConfig(string configName)
    {
        JsonSerializerOptions jsonSerializerOptions = AppJsonSettings.GetDefaultOptions();
        var defaultConfig = new DefaultConfig
        {
            DefaultConfigPath = GetRabbitMqConfigPath(configName)
        };
        string defaultConfigJson = JsonSerializer.Serialize(defaultConfig, jsonSerializerOptions);
        File.WriteAllText(settings.DefaultConfigFile, defaultConfigJson);
    }
    
    public bool RabbitMqConfigExists(string configName) => File.Exists(GetRabbitMqConfigPath(configName));

    public string SaveRabbitMqConfig(RabbitMqConfigData configData, string configName)
    {
        string configPath = GetRabbitMqConfigPath(configName);
        if (File.Exists(configPath))
        {
            throw new Exception("Config file already exists.");
        }
        
        JsonSerializerOptions jsonSerializerOptions = AppJsonSettings.GetDefaultOptions();
        string json = JsonSerializer.Serialize(configData, jsonSerializerOptions);
        File.WriteAllText(configPath, json);

        return configPath;
    }

    public RabbitMqConfigData GetDefaultRabbitMqConfig() => GetRabbitMqConfig(GetDefaultRabbitMqConfigName());

    public RabbitMqConfigData GetRabbitMqConfig(string configName)
    {
        string rabbitMqConfigPath = GetRabbitMqConfigPath(configName);

        if (!File.Exists(rabbitMqConfigPath))
        {
            throw new Exception($"Config file not found: {rabbitMqConfigPath}");
        }
        
        JsonSerializerOptions jsonSerializerOptions = AppJsonSettings.GetDefaultOptions();

        try
        {
            string rabbitMqConfigJson = File.ReadAllText(rabbitMqConfigPath);
            var configData = JsonSerializer.Deserialize<RabbitMqConfigData>(rabbitMqConfigJson, jsonSerializerOptions)!;
            return configData;
        }
        catch (Exception e)
        {
            throw new Exception($"Could not load config file: {rabbitMqConfigPath}", e);
        }
    }

    public string GetDefaultRabbitMqConfigName()
    {
        string rabbitMqConfigJson = File.ReadAllText(settings.DefaultConfigFile);
        var defaultConfig = JsonSerializer.Deserialize<DefaultConfig>(rabbitMqConfigJson, AppJsonSettings.GetDefaultOptions());
        if (defaultConfig is null) return string.Empty;

        return new FileInfo(defaultConfig.DefaultConfigPath).Name.Replace(".json", "");
    }
    
    private string GetRabbitMqConfigPath(string configName) 
        => Path.Combine(settings.ConfigDirectory, $"{configName}.json");
}