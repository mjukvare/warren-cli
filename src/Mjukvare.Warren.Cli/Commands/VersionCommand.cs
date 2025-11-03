using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mjukvare.Warren.Cli.Commands;

public class VersionCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
            
        AnsiConsole.MarkupLine($"[blue]warren[/] version [green]{version}[/]");
        return 0;
    }
}