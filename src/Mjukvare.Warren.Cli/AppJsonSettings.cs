using System.Text.Json;

namespace Mjukvare.Warren.Cli;

public static class AppJsonSettings
{
    public static JsonSerializerOptions GetDefaultOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}