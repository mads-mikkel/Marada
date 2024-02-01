using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Threading.Channels;

using static System.Console;

namespace Marada.Cli;

partial class CliEntry
{
    static string version = "1.0";
    static string description = "MARADA Cli";

    public static async Task<int> Main(string[] args)
    {
        OutputEncoding = System.Text.Encoding.Unicode;
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


    static string currentOutput = String.Empty;
    internal static void Display(string message, bool startNewLine = false, DisplayKind displayEnum = DisplayKind.Normal)
    {
        switch(displayEnum)
        {
            case DisplayKind.ExternalTool:
                ForegroundColor = ConsoleColor.Blue;
                break;
            case DisplayKind.Warning:
                ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case DisplayKind.Error:
                ForegroundColor = ConsoleColor.DarkRed;
                break;
            case DisplayKind.Normal:
            default:
                ForegroundColor = ConsoleColor.DarkCyan;
                break;
        }

        string line = currentOutput;
        if(startNewLine)
        {
            line = $"{Environment.NewLine}▶ {message}";
        }
        else
        {
            line = $" {message}";
        }
        currentOutput = line;

        Write(line);
        ForegroundColor = ConsoleColor.White;
    }

    internal static void ExecuteSystemInfoCommand()
    {
        WriteLine($"{DateTime.Now}");
    }

    internal static void ExecuteBackendCommand(string backendName, string dbconn)
    {
        string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @$"source\repos\sandbox\clitest\{backendName}");
        BackendTemplateCreator backendTemplateCreator = new(rootPath, backendName);

        Display("Checking for installed software: dotnet, git... ", startNewLine: true);
        (string m, bool succes) result = backendTemplateCreator.CheckInstalledSoftware();
        OutputResult(result);

        Display($"Creating folder structure in {rootPath}... ", startNewLine: true);
        result = backendTemplateCreator.CreateFolderStructure(backendName);
        OutputResult(result);

        Display($"Creating {backendName}.sln file in {rootPath}\\src... ", startNewLine: true);
        result = backendTemplateCreator.CreateSolutionFile();
        OutputResult(result);

        Display($"Creating projects in {rootPath}\\src... ", startNewLine: true);
        result = backendTemplateCreator.CreateProjects();
        OutputResult(result);
    }

    private static void OutputResult((string m, bool success) result)
    {
        if(result.success)
        {
            Display(result.m, startNewLine: false);
        }
        else
        {
            Display(result.m, startNewLine: true, DisplayKind.Error);
        }
    }
}

internal class BackendTemplateCreator
{
    private readonly string rootPath;
    private readonly string backendName;

    public BackendTemplateCreator(string rootPath, string backendName)
    {
        this.rootPath = rootPath;
        this.backendName = backendName;
    }

    public (string, bool) CheckInstalledSoftware()
    {
        try
        {
            var programsPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var dotnetExe = "dotnet.exe";
            var gitExe = "git-cmd.exe";

            var dotnetPath = Path.Combine(programsPath, "dotnet");
            var gitPath = Path.Combine(programsPath, "Git");

            var dotnetFiles = Directory.GetFiles(dotnetPath, dotnetExe);
            var gitFiles = Directory.GetFiles(gitPath, gitExe);

            if(dotnetFiles.FirstOrDefault(s => s.EndsWith(dotnetExe)) is null)
            {
                return ("dotnet is not installed. The template commmand of MARADA Cli, requires dotnet to be installed.", false);
            }
            if(gitFiles.FirstOrDefault(s => s.EndsWith(gitExe)) is null)
            {
                return ("git is not installed. The template commmand of MARADA Cli, requires git to be installed.", false);
            }
        }
        catch(Exception e)
        {
            return (e.Message, false);
        }
        return ("dotnet and git are installed.", true);
    }

    public (string, bool) CreateFolderStructure(string name)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(rootPath, "src"));
            Directory.CreateDirectory(Path.Combine(rootPath, "docs"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Infrastructure"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Infrastructure\EfContexts"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Infrastructure\DataAccess"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Presentation"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Application"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Application\Dto"));
            Directory.CreateDirectory(Path.Combine(rootPath, @"src\Domain"));
        }
        catch(Exception e)
        {
            return (e.Message, false);
        }
        return ("Done", true);
    }

    public (string, bool) CreateSolutionFile()
    {
        string command = $"new sln -n {backendName} -o {Path.Combine(rootPath, "src")}";
        return ExecuteDotnet(command);
    }

    public (string, bool) CreateProjects()
    {
        string projectName = $"{backendName}.Infrastructure.EfContexts";
        string srcFolder = Path.Combine(rootPath, "src");
        string dir = Path.Combine(srcFolder, "Infrastructure\\EfContexts");
        (string m, bool success) result = new();
        string trace = "";

        string command = $"new classlib -n {projectName} -o {dir} --force";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"build {dir}";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        projectName = $"{backendName}.Infrastructure.DataAccess";
        dir = Path.Combine(srcFolder, "Infrastructure\\DataAccess");
        command = $"new classlib -n {projectName} -o {dir} --force";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"build {dir}";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        projectName = $"{backendName}.Application.Dto";
        dir = Path.Combine(srcFolder, "Application\\Dto");
        command = $"new classlib -n {projectName} -o {dir} --force";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"build {dir}";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"new classlib -n {projectName} -o {dir} --force";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"build {dir}";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"new console -n {backendName}.temprunner -o {srcFolder} --force";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"build {dir}";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        command = $"ef dbcontext scaffold \"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\" " +
            $"Microsoft.EntityFrameworkCore.SqlServer " +
            "-v " +
            $"-c " + backendName + "Context " +
            "--context-dir Infrastructure/EfContexts " +
            $"-o Application/Dto " +
            $"-p \"{Path.Combine(srcFolder, "Application\\Dto\\")}" + $"{backendName}" + ".Application.Dto.csproj\" " +
            "-s " + backendName + ".temprunner ";
        result = ExecuteDotnet(command);
        trace += result.m + Environment.NewLine;
        if(!result.success) return result;

        //command = $"new classlib -n {backendName}.Application.Dto -o {Path.Combine(srcFolder, "Application\\Dto")} --force";
        //result = ExecuteDotnet(command);
        //if(!result.success) return result;

        return (trace, true);
    }

    private (string, bool) ExecuteDotnet(string argument)
    {
        try
        {

            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                FileName = "dotnet.exe",
                Arguments = argument,
            };
            process.StartInfo = startInfo;
            process.Start();
            string line;
            while((line = process.StandardOutput.ReadLine()) != null)
            {
                CliEntry.Display("dotnet says: " + line, true, DisplayKind.ExternalTool);
            }
            process.WaitForExit();
        }
        catch(Exception e)
        {
            return ($"Error executing > dotnet {argument}: " + e.Message, false);
        }
        return ($"Done executing > dotnet {argument}", true);
    }

}

internal class DotnetExecutor
{

}