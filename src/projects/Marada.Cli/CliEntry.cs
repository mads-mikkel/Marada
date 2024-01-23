using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;

using static System.Console;

namespace Marada.Cli;

class CliEntry
{
    static string version = "1.0";
    static string description = "MARADA Cli";

    public static async Task<int> Main(string[] args)
    {
        var systemInfoCommand = new Command("info", "Displays systeminfo");

        var templateCommand = new Command("template", "Creates a template.");
        var backendTemplateOption = new Option<string>(
            name: "--backend",
            description: "Creates a backend template with the provided argument as its name."
            );
        var argumentDbConn = new Argument<string>(
            name: "db",
            description: "The connection string for the database to use.");

        templateCommand.AddOption(backendTemplateOption);
        templateCommand.AddArgument(argumentDbConn);
        
        var rootCommand = new RootCommand(description);
        
        rootCommand.AddCommand(templateCommand);
        rootCommand.AddCommand(systemInfoCommand);

        systemInfoCommand.SetHandler(() => ExecuteSystemInfoCommand());

        templateCommand.SetHandler(ExecuteBackendCommand,
            argumentDbConn,
            backendTemplateOption
        );

        return await rootCommand.InvokeAsync(args);
    }

    internal static void ExecuteSystemInfoCommand()
    {
        WriteLine($"{DateTime.Now}");
    }

    internal static async Task ExecuteBackendCommand(string backendName, string dbconn)
    {
        WriteLine($"mrd: {backendName}, {dbconn}");
    }
}