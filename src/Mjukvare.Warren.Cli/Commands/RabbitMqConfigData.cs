namespace Mjukvare.Warren.Cli.Commands;

public class RabbitMqConfigData
{
    public required string HostName { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string VirtualHost { get; init; }
}