namespace Mjukvare.Warren.Cli;

public sealed record AppSettings
{
    public required string ConfigDirectory { get; init; }
    public required string? DefaultConfigPath { get; set; }

    public string DefaultConfigFile => Path.Combine(ConfigDirectory, "config.json");
}