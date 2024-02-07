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
        string rootPath = @$"C:\temp\{backendName}";    //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @$"source\repos\sandbox\clitest\{backendName}");
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

        Display($"Building projects in {rootPath}\\src... ", startNewLine: true);
        result = backendTemplateCreator.BuildProjects();
        OutputResult(result);

        Display($"Installing EF NuGets...", startNewLine: true);
        result = backendTemplateCreator.InstallDesignNuget();
        OutputResult(result);
        result = backendTemplateCreator.InstallSqlServerNuget();
        OutputResult(result);

        string c = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        Display($"Running EF...", startNewLine: true);
        result = backendTemplateCreator.RunEf(c);
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
    //private readonly List<string> paths = new();
    private readonly Dictionary<string, string> projectsPaths = new();

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
            projectsPaths.Add("srcFolder", Path.Combine(rootPath, "src"));
            projectsPaths.Add("docsFolder", Path.Combine(rootPath, "docs"));
            projectsPaths.Add("Infrastructure", Path.Combine(rootPath, @"src\Infrastructure"));
            projectsPaths.Add("EfContexts", Path.Combine(rootPath, @"src\Infrastructure\EfContexts"));
            projectsPaths.Add("DataAccess", Path.Combine(rootPath, @"src\Infrastructure\DataAccess"));
            projectsPaths.Add("Presentation", Path.Combine(rootPath, @"src\Presentation"));
            projectsPaths.Add("Application", Path.Combine(rootPath, @"src\Application"));
            projectsPaths.Add("Dto", Path.Combine(rootPath, @"src\Application\Dto"));
            projectsPaths.Add("Domain", Path.Combine(rootPath, @"src\Domain"));
            projectsPaths.Add("tmp", Path.Combine(rootPath, @"src\tmp"));
            foreach(var path in projectsPaths.Values)
            {
                Directory.CreateDirectory(path);
            }
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
        (string m, bool success) result = CreateProject(projectsPaths["EfContexts"], $"{backendName}.Infrastructure.EfContexts", "classlib");
        result = CreateProject(projectsPaths["DataAccess"], $"{backendName}.Infrastructure.DataAccess", "classlib");
        result = CreateProject(projectsPaths["Dto"], $"{backendName}.Application.Dto", "classlib");
        result = CreateProject(projectsPaths["tmp"], $"{backendName}.temprunner", "console");
        return ("All projects created successfully", true);
    }

    public (string, bool) BuildProjects()
    {
        (string m, bool success) result = BuildProject(projectsPaths["EfContexts"]);
        result = BuildProject(projectsPaths["DataAccess"]);
        result = BuildProject(projectsPaths["Dto"]);
        result = BuildProject(projectsPaths["tmp"]);
        return ("All projects successfully builded", true);
    }

    public (string, bool) InstallDesignNuget()
    {
        string command = $"add {projectsPaths["tmp"]}\\{backendName}.temprunner.csproj package Microsoft.EntityFrameworkCore.Design";
        (string m, bool success) = ExecuteDotnet(command);
        return (m, success);
    }

    public (string, bool) InstallSqlServerNuget()
    {
        // {projectsPaths["EfContexts"]}\\{backendName}.Infrastructure.EfContexts.csproj
        string command = $"add {projectsPaths["tmp"]}\\{backendName}.temprunner.csproj package Microsoft.EntityFrameworkCore.SqlServer";
        (string m, bool success) = ExecuteDotnet(command);
        return (m, success);
    }

    public (string, bool) RunEf(string connectionString)
    {
        string command = $"ef dbcontext scaffold \"{connectionString}\" Microsoft.EntityFrameworkCore.SqlServer " +
            "-v " +
            $"-c {backendName}Context " +
            $"--context-dir {projectsPaths["EfContexts"]} " +
            $"-o {projectsPaths["Dto"]} " +
            $"-p {projectsPaths["tmp"]} " +
            $"-s {projectsPaths["tmp"]}\\{backendName}.temprunner.csproj ";
        (string m, bool success) = ExecuteDotnet(command);

        return (m, success);
    }

    private (string, bool) CreateProject(string fullpath, string name, string type)
    {
        string command = $"new {type} -n {name} -o {fullpath} --force";
        (string m, bool success) result = ExecuteDotnet(command);
        string trace = result.m + Environment.NewLine;
        return result;
    }

    private (string, bool) BuildProject(string fullPath)
    {
        string command = $"build {fullPath}";
        (string m, bool success) result = ExecuteDotnet(command);
        string trace = result.m + Environment.NewLine;
        return result;
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